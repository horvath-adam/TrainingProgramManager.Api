using Microsoft.EntityFrameworkCore;
using TrainingProgramManager.Api.Contracts.Workshops;
using TrainingProgramManager.Api.Data;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Workshops
{
    public class WorkshopRepository : IWorkshopRepository
    {
        private readonly TrainingProgramDbContext _dbContext;

        public WorkshopRepository(TrainingProgramDbContext dbContext)
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

        // EN: The global soft-delete query filter stays active (no IgnoreQueryFilters), and ordering plus DTO projection run in the database; only the read model is returned, never entities.
        // HU: A globális soft delete query filter aktív marad (nincs IgnoreQueryFilters), a rendezés és a DTO projekció az adatbázisban fut; csak az olvasási modell kerül vissza, entitás soha.
        public async Task<IReadOnlyCollection<WorkshopListItemResponse>> GetPublicListAsync() =>
            await _dbContext.Workshops
                .AsNoTracking()
                .OrderBy(w => w.Title)
                .ThenBy(w => w.Id)
                .Select(w => new WorkshopListItemResponse(
                    w.Id,
                    w.Title,
                    w.StartsAt,
                    w.EndsAt,
                    w.Event.Name,
                    w.Room.Name,
                    w.Speaker.FullName))
                .ToListAsync();

        // EN: The repository filters by OrganizerId in SQL, so only the owner's visible (not soft-deleted) workshops are loaded. The owner id is received from the service, never read from HttpContext.
        // HU: A repository SQL-ben szűr az OrganizerId-ra, így csak a tulajdonos látható (nem soft delete-elt) workshopjai töltődnek be. A tulajdonos azonosítóját a service adja át, a repository nem olvassa HttpContext-ből.
        // EN: Ordering by Id makes the result deterministic, which keeps demonstrations reproducible.
        // HU: Az Id szerinti rendezés determinisztikussá teszi az eredményt, így a bemutatók megismételhetők.
        public async Task<IReadOnlyCollection<WorkshopListItemResponse>> GetByOrganizerAsync(string organizerId) =>
            await _dbContext.Workshops
                .AsNoTracking()
                .Where(w => w.OrganizerId == organizerId)
                .OrderBy(w => w.Id)
                .Select(w => new WorkshopListItemResponse(
                    w.Id,
                    w.Title,
                    w.StartsAt,
                    w.EndsAt,
                    w.Event.Name,
                    w.Room.Name,
                    w.Speaker.FullName))
                .ToListAsync();

        public Task<WorkshopDetailsResponse?> GetDetailsByIdAsync(int id) =>
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

        // EN: The entity is intentionally tracked because the service will modify or remove it.
        // HU: Az entitást szándékosan követjük, mert a service módosítani vagy törölni fogja.
        public Task<Workshop?> GetEntityByIdAsync(int id) =>
            _dbContext.Workshops.FirstOrDefaultAsync(w => w.Id == id);

        public Task<bool> EventExistsAsync(int eventId) =>
            _dbContext.Events.AnyAsync(e => e.Id == eventId);

        public Task<bool> RoomExistsAsync(int roomId) =>
            _dbContext.Rooms.AnyAsync(r => r.Id == roomId);

        public Task<bool> SpeakerExistsAsync(int speakerId) =>
            _dbContext.Speakers.AnyAsync(s => s.Id == speakerId);

        public async Task<bool> RoomHasOverlappingWorkshopAsync(
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

        public void Add(Workshop workshop) => _dbContext.Workshops.Add(workshop);

        public void Remove(Workshop workshop) => _dbContext.Workshops.Remove(workshop);

        public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();
    }
}
