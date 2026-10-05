using TrainingProgramManager.Api.Contracts.Workshops;

namespace TrainingProgramManager.Api.Services.Workshops
{
    public interface IWorkshopRepository
    {
        Task<IReadOnlyCollection<WorkshopListItemResponse>> GetAllAsync(int? eventId = null);

        Task<WorkshopDetailsResponse?> GetDetailsByIdAsync(int id);

        Task<IReadOnlyCollection<string>?> GetTagsAsync(int id);
    }
}
