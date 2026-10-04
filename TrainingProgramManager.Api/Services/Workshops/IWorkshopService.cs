using TrainingProgramManager.Api.Contracts.Workshops;

namespace TrainingProgramManager.Api.Services.Workshops
{
    public interface IWorkshopService
    {
        Task<IReadOnlyCollection<WorkshopListItemResponse>> GetAllAsync(
            int? eventId = null);

        Task<WorkshopDetailsResponse?> GetByIdAsync(int id);

        Task<IReadOnlyCollection<string>?> GetTagsAsync(int id);
    }
}
