using TrainingProgramManager.Api.Contracts.Workshops;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Workshops
{
    public interface IWorkshopRepository
    {
        Task<IReadOnlyCollection<WorkshopListItemResponse>> GetAllAsync(int? eventId = null);

        Task<WorkshopDetailsResponse?> GetDetailsByIdAsync(int id);

        Task<IReadOnlyCollection<string>?> GetTagsAsync(int id);

        Task<Workshop?> GetEntityByIdAsync(int id);

        Task<bool> EventExistsAsync(int eventId);

        Task<bool> RoomExistsAsync(int roomId);

        Task<bool> SpeakerExistsAsync(int speakerId);

        Task<bool> RoomHasOverlappingWorkshopAsync(
            int roomId,
            DateTimeOffset startsAt,
            DateTimeOffset endsAt,
            int? ignoredWorkshopId = null);

        void Add(Workshop workshop);

        void Remove(Workshop workshop);

        Task SaveChangesAsync();
    }
}
