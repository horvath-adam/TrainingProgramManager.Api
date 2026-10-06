using TrainingProgramManager.Api.Contracts.Workshops;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Workshops
{
    public class WorkshopService : IWorkshopService
    {
        private readonly IWorkshopRepository _workshopRepository;

        public WorkshopService(IWorkshopRepository workshopRepository)
        {
            _workshopRepository = workshopRepository;
        }

        public Task<IReadOnlyCollection<WorkshopListItemResponse>> GetAllAsync(int? eventId = null) =>
            _workshopRepository.GetAllAsync(eventId);

        public Task<WorkshopDetailsResponse?> GetByIdAsync(int id) =>
            _workshopRepository.GetDetailsByIdAsync(id);

        public Task<IReadOnlyCollection<string>?> GetTagsAsync(int id) =>
            _workshopRepository.GetTagsAsync(id);

        public async Task<ServiceResult<WorkshopDetailsResponse>> CreateAsync(CreateWorkshopRequest request)
        {
            if (HasInvalidTimeRange(request.StartsAt, request.EndsAt))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure(
                    ServiceErrorType.Validation, "The workshop start time must be earlier than the end time.");
            }

            if (!await _workshopRepository.EventExistsAsync(request.EventId))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure(ServiceErrorType.Validation, "The selected event does not exist.");
            }

            if (!await _workshopRepository.RoomExistsAsync(request.RoomId))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure(ServiceErrorType.Validation, "The selected room does not exist.");
            }

            if (!await _workshopRepository.SpeakerExistsAsync(request.SpeakerId))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure(ServiceErrorType.Validation, "The selected speaker does not exist.");
            }

            if (await _workshopRepository.RoomHasOverlappingWorkshopAsync(
                request.RoomId, request.StartsAt, request.EndsAt))
            {
                return ServiceResult<WorkshopDetailsResponse>.Failure(
                    ServiceErrorType.Conflict, "The selected room is already booked in this time period.");
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

            // EN: The repository only tracks the entity; the service decides when to save.
            // HU: A repository csak követi az entitást; a service dönti el, mikor történik a mentés.
            _workshopRepository.Add(workshop);
            await _workshopRepository.SaveChangesAsync();

            var response = await _workshopRepository.GetDetailsByIdAsync(workshop.Id);

            return ServiceResult<WorkshopDetailsResponse>.Success(response!);
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateWorkshopRequest request)
        {
            if (HasInvalidTimeRange(request.StartsAt, request.EndsAt))
            {
                return ServiceResult.Failure(ServiceErrorType.Validation, "The workshop start time must be earlier than the end time.");
            }

            var workshop = await _workshopRepository.GetEntityByIdAsync(id);

            if (workshop is null)
            {
                return ServiceResult.Failure(ServiceErrorType.NotFound, "Workshop not found.");
            }

            if (!await _workshopRepository.EventExistsAsync(request.EventId))
            {
                return ServiceResult.Failure(ServiceErrorType.Validation, "The selected event does not exist.");
            }

            if (!await _workshopRepository.RoomExistsAsync(request.RoomId))
            {
                return ServiceResult.Failure(ServiceErrorType.Validation, "The selected room does not exist.");
            }

            if (!await _workshopRepository.SpeakerExistsAsync(request.SpeakerId))
            {
                return ServiceResult.Failure(ServiceErrorType.Validation, "The selected speaker does not exist.");
            }

            if (await _workshopRepository.RoomHasOverlappingWorkshopAsync(
                request.RoomId, request.StartsAt, request.EndsAt, id))
            {
                return ServiceResult.Failure(ServiceErrorType.Conflict, "The selected room is already booked in this time period.");
            }

            workshop.Title = request.Title;
            workshop.EventId = request.EventId;
            workshop.RoomId = request.RoomId;
            workshop.SpeakerId = request.SpeakerId;
            workshop.StartsAt = request.StartsAt;
            workshop.EndsAt = request.EndsAt;

            await _workshopRepository.SaveChangesAsync();

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var workshop = await _workshopRepository.GetEntityByIdAsync(id);

            if (workshop is null)
            {
                return ServiceResult.Failure(ServiceErrorType.NotFound, "Workshop not found.");
            }

            // EN: Soft delete: mark the workshop as deleted instead of removing the row.
            // HU: Soft delete: a sort nem töröljük, csak töröltnek jelöljük a workshopot.
            workshop.IsDeleted = true;
            workshop.DeletedAt = DateTimeOffset.UtcNow;
            await _workshopRepository.SaveChangesAsync();

            return ServiceResult.Success();
        }

        private static bool HasInvalidTimeRange(DateTimeOffset startsAt, DateTimeOffset endsAt) =>
            startsAt >= endsAt;
    }
}
