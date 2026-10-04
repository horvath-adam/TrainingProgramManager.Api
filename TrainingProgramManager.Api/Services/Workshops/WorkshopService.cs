using Microsoft.EntityFrameworkCore;
using TrainingProgramManager.Api.Contracts.Workshops;
using TrainingProgramManager.Api.Data;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Workshops
{
    public class WorkshopService : IWorkshopService
    {
        private readonly TrainingProgramDbContext _dbContext;

        public WorkshopService(TrainingProgramDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<WorkshopListItemResponse>> GetAllAsync(int? eventId = null)
        {
            // EN: AsNoTracking() states the read intent; the DTO projection itself does not create tracked entities.
            // HU: Az AsNoTracking() jelzi az olvasási szándékot; a DTO projekció önmagában nem hoz létre követett entitásokat.
            var query = _dbContext.Workshops.AsNoTracking();

            if (eventId is not null)
            {
                query = query.Where(w => w.EventId == eventId);
            }

            var workshops = await query
                .Select(w => new WorkshopListItemResponse(
                    w.Id,
                    w.Title,
                    w.StartsAt,
                    w.EndsAt,
                    w.Event.Name,
                    w.Room.Name,
                    w.Speaker.FullName))
                .ToListAsync();

            // EN: The SQLite provider cannot translate ORDER BY on DateTimeOffset, so the small projected result is ordered in memory.
            // HU: Az SQLite provider nem tudja lefordítani a DateTimeOffset szerinti ORDER BY-t, ezért a kis méretű projektált eredményt memóriában rendezzük.
            return workshops
                .OrderBy(w => w.StartsAt)
                .ThenBy(w => w.Id)
                .ToList();
        }

        public Task<WorkshopDetailsResponse?> GetByIdAsync(int id) => GetWorkshopDetailsByIdAsync(id);

        public async Task<IReadOnlyCollection<string>?> GetTagsAsync(int id)
        {
            var workshopExists = await _dbContext.Workshops.AnyAsync(w => w.Id == id);

            if (!workshopExists)
            {
                return null;
            }

            return await _dbContext.Workshops
                .AsNoTracking()
                .Where(w => w.Id == id)
                .SelectMany(w => w.Tags)
                .Select(t => t.Name)
                .OrderBy(name => name)
                .ToListAsync();
        }

        public async Task<ServiceResult<WorkshopDetailsResponse>> CreateAsync(CreateWorkshopRequest request)
        {
            if (HasInvalidTimeRange(request.StartsAt, request.EndsAt))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure(
                    "The workshop start time must be earlier than the end time.");
            }

            if (!await _dbContext.Events.AnyAsync(e => e.Id == request.EventId))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure("The selected event does not exist.");
            }

            if (!await _dbContext.Rooms.AnyAsync(r => r.Id == request.RoomId))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure("The selected room does not exist.");
            }

            if (!await _dbContext.Speakers.AnyAsync(s => s.Id == request.SpeakerId))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure("The selected speaker does not exist.");
            }

            if (await RoomHasOverlappingWorkshopAsync(request.RoomId, request.StartsAt, request.EndsAt))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure(
                    "The selected room is already booked in this time period.");
            }

            var workshop = new Workshop
            {
                Title = request.Title,
                EventId = request.EventId,
                RoomId = request.RoomId,
                SpeakerId = request.SpeakerId,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt
            };

            _dbContext.Workshops.Add(workshop);
            await _dbContext.SaveChangesAsync();

            var response = await GetWorkshopDetailsByIdAsync(workshop.Id);

            return ServiceResult<WorkshopDetailsResponse>.Success(response!);
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateWorkshopRequest request)
        {
            if (HasInvalidTimeRange(request.StartsAt, request.EndsAt))
            {
                return ServiceResult.Failure("The workshop start time must be earlier than the end time.");
            }

            var workshop = await _dbContext.Workshops.FirstOrDefaultAsync(w => w.Id == id);

            if (workshop is null)
            {
                return ServiceResult.Failure("Workshop not found.");
            }

            if (!await _dbContext.Events.AnyAsync(e => e.Id == request.EventId))
            {
                return ServiceResult.Failure("The selected event does not exist.");
            }

            if (!await _dbContext.Rooms.AnyAsync(r => r.Id == request.RoomId))
            {
                return ServiceResult.Failure("The selected room does not exist.");
            }

            if (!await _dbContext.Speakers.AnyAsync(s => s.Id == request.SpeakerId))
            {
                return ServiceResult.Failure("The selected speaker does not exist.");
            }

            if (await RoomHasOverlappingWorkshopAsync(request.RoomId, request.StartsAt, request.EndsAt, id))
            {
                return ServiceResult.Failure("The selected room is already booked in this time period.");
            }

            workshop.Title = request.Title;
            workshop.EventId = request.EventId;
            workshop.RoomId = request.RoomId;
            workshop.SpeakerId = request.SpeakerId;
            workshop.StartsAt = request.StartsAt;
            workshop.EndsAt = request.EndsAt;

            await _dbContext.SaveChangesAsync();

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var workshop = await _dbContext.Workshops.FirstOrDefaultAsync(w => w.Id == id);

            if (workshop is null)
            {
                return ServiceResult.Failure("Workshop not found.");
            }

            _dbContext.Workshops.Remove(workshop);
            await _dbContext.SaveChangesAsync();

            return ServiceResult.Success();
        }

        private static bool HasInvalidTimeRange(DateTimeOffset startsAt, DateTimeOffset endsAt) =>
            startsAt >= endsAt;

        private async Task<bool> RoomHasOverlappingWorkshopAsync(
            int roomId,
            DateTimeOffset startsAt,
            DateTimeOffset endsAt,
            int? ignoredWorkshopId = null)
        {
            var query = _dbContext.Workshops
                .AsNoTracking()
                .Where(w => w.RoomId == roomId);

            if (ignoredWorkshopId is not null)
            {
                query = query.Where(w => w.Id != ignoredWorkshopId);
            }

            // EN: SQLite cannot compare DateTimeOffset in SQL, so only the small time-range projection is loaded and compared in memory.
            // HU: Az SQLite nem tud DateTimeOffset értékeket SQL-ben összehasonlítani, ezért csak a kis időtartomány-projekciót töltjük be, és memóriában hasonlítjuk össze.
            var candidates = await query
                .Select(w => new { w.StartsAt, w.EndsAt })
                .ToListAsync();

            // EN: This check and the later SaveChangesAsync() are separate operations, so it is not a concurrency-safe booking guarantee.
            // HU: Ez az ellenőrzés és a későbbi SaveChangesAsync() külön műveletek, ezért ez nem garantálja az egyidejű foglalások biztonságát.
            return candidates.Any(w => w.StartsAt < endsAt && startsAt < w.EndsAt);
        }

        private Task<WorkshopDetailsResponse?> GetWorkshopDetailsByIdAsync(int id) =>
            _dbContext.Workshops
                .AsNoTracking()
                .Where(w => w.Id == id)
                .Select(w => new WorkshopDetailsResponse(
                    w.Id,
                    w.Title,
                    w.StartsAt,
                    w.EndsAt,
                    w.EventId,
                    w.Event.Name,
                    w.RoomId,
                    w.Room.Name,
                    w.SpeakerId,
                    w.Speaker.FullName,
                    w.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList()))
                .FirstOrDefaultAsync();
    }
}
