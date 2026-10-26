using Microsoft.EntityFrameworkCore;
using TrainingProgramManager.Api.Caching;
using Microsoft.Extensions.Caching.Memory;
using TrainingProgramManager.Api.Contracts.Workshops;
using TrainingProgramManager.Api.Entities;
using TrainingProgramManager.Api.Security;

namespace TrainingProgramManager.Api.Services.Workshops
{
    public class WorkshopService : IWorkshopService
    {
        private readonly IWorkshopRepository _workshopRepository;
        private readonly ICurrentUserService _currentUserService;

        private readonly ILogger<WorkshopService> _logger;
        private readonly IMemoryCache _memoryCache;

        public WorkshopService(
            IWorkshopRepository workshopRepository,
            ICurrentUserService currentUserService,
            ILogger<WorkshopService> logger,
            IMemoryCache memoryCache)
        {
            _workshopRepository = workshopRepository;
            _currentUserService = currentUserService;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public Task<IReadOnlyCollection<WorkshopListItemResponse>> GetAllAsync(int? eventId = null) =>
            _workshopRepository.GetAllAsync(eventId);

        // EN: The database is queried only on a cache miss; the cached value is the DTO collection (an empty list is a valid, cached result), and it expires after two minutes.
        // HU: Az adatbázis lekérdezése csak cache miss esetén történik; a gyorsítótárazott érték a DTO gyűjtemény (az üres lista érvényes, gyorsítótárazott eredmény), és két perc után lejár.
        public async Task<IReadOnlyCollection<WorkshopListItemResponse>> GetPublicListAsync()
        {
            var workshops = await _memoryCache.GetOrCreateAsync(
                CacheKeys.PublicWorkshops,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

                    var result = await _workshopRepository.GetPublicListAsync();

                    _logger.LogInformation(
                        "Public workshop list loaded from database and cached.");

                    return result;
                });

            return workshops ?? [];
        }

        // EN: The current user id is resolved here from the validated JWT, so a caller can never ask for another Organizer's workshops.
        // HU: Az aktuális felhasználó azonosítóját itt a validált JWT-ből határozzuk meg, így a hívó soha nem kérheti másik szervező workshopjait.
        public Task<IReadOnlyCollection<WorkshopListItemResponse>> GetMineAsync() =>
            _workshopRepository.GetByOrganizerAsync(_currentUserService.GetRequiredUserId());

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
                EndsAt = request.EndsAt,
                // EN: The owner is the authenticated current user taken from the validated JWT, never from the request, so a client cannot create workshops in someone else's name.
                // HU: A tulajdonos a validált JWT-ből vett hitelesített aktuális felhasználó, soha nem a kérésből, így a kliens nem hozhat létre workshopot más nevében.
                OrganizerId = _currentUserService.GetRequiredUserId()
            };

            // EN: The repository only tracks the entity; the service decides when to save.
            // HU: A repository csak követi az entitást; a service dönti el, mikor történik a mentés.
            _workshopRepository.Add(workshop);

            try
            {
                await _workshopRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // EN: The full exception stays in the server log; the caller only gets a generic message.
                // HU: A teljes kivétel a szerver naplójában marad; a hívó csak általános üzenetet kap.
                _logger.LogError(ex, "Database error while creating Workshop for user {UserId}.", workshop.OrganizerId);

                return ServiceResult<WorkshopDetailsResponse>.Failure(
                    ServiceErrorType.PersistenceFailure, "The workshop could not be created.");
            }

            // EN: The cached public list is removed only after the new Workshop was saved, so a failed create never discards a still-valid cache entry.
            // HU: A gyorsítótárazott publikus listát csak az új Workshop mentése után töröljük, így egy sikertelen létrehozás nem dob el még érvényes gyorsítótár-bejegyzést.
            _memoryCache.Remove(CacheKeys.PublicWorkshops);

            var response = await _workshopRepository.GetDetailsByIdAsync(workshop.Id);

            _logger.LogInformation("Workshop {WorkshopId} created by user {UserId}.", workshop.Id, workshop.OrganizerId);

            return ServiceResult<WorkshopDetailsResponse>.Success(response!);
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateWorkshopRequest request)
        {
            var workshop = await _workshopRepository.GetEntityByIdAsync(id);

            if (workshop is null)
            {
                return ServiceResult.Failure(ServiceErrorType.NotFound, "Workshop not found.");
            }

            // EN: Existence and ownership are checked before business validation, so a non-owner gets Forbidden instead of validation details about someone else's workshop.
            // HU: A létezést és a tulajdonjogot az üzleti validáció előtt ellenőrizzük, így a nem tulajdonos Forbidden választ kap, és nem kap validációs részleteket más workshopjáról.
            var currentUserId = _currentUserService.GetRequiredUserId();

            if (workshop.OrganizerId != currentUserId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to modify Workshop {WorkshopId} owned by another user.", currentUserId, id);

                return ServiceResult.Failure(ServiceErrorType.Forbidden, "You cannot modify this workshop.");
            }

            if (HasInvalidTimeRange(request.StartsAt, request.EndsAt))
            {
                return ServiceResult.Failure(ServiceErrorType.Validation, "The workshop start time must be earlier than the end time.");
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

            // EN: The cached public list is removed only after the update was saved, so it never serves data older than the database.
            // HU: A gyorsítótárazott publikus listát csak a módosítás mentése után töröljük, így nem szolgál ki az adatbázisnál régebbi adatot.
            _memoryCache.Remove(CacheKeys.PublicWorkshops);

            _logger.LogInformation("Workshop {WorkshopId} updated by user {UserId}.", workshop.Id, currentUserId);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var workshop = await _workshopRepository.GetEntityByIdAsync(id);

            if (workshop is null)
            {
                return ServiceResult.Failure(ServiceErrorType.NotFound, "Workshop not found.");
            }

            // EN: Soft delete is still a delete, so it must enforce ownership: an Organizer cannot remove another Organizer's workshop.
            // HU: A soft delete is törlésnek számít, ezért érvényesíteni kell a tulajdonjogot: egy szervező nem törölheti másik szervező workshopját.
            if (workshop.OrganizerId != _currentUserService.GetRequiredUserId())
            {
                return ServiceResult.Failure(ServiceErrorType.Forbidden, "You cannot delete this workshop.");
            }

            // EN: Soft delete: mark the workshop as deleted instead of removing the row.
            // HU: Soft delete: a sort nem töröljük, csak töröltnek jelöljük a workshopot.
            workshop.IsDeleted = true;
            workshop.DeletedAt = DateTimeOffset.UtcNow;
            await _workshopRepository.SaveChangesAsync();

            // EN: The soft-deleted workshop must disappear from the public list, so the cache is cleared after the successful save.
            // HU: A soft delete-tel törölt workshopnak el kell tűnnie a publikus listából, ezért a gyorsítótárat a sikeres mentés után töröljük.
            _memoryCache.Remove(CacheKeys.PublicWorkshops);

            return ServiceResult.Success();
        }

        private static bool HasInvalidTimeRange(DateTimeOffset startsAt, DateTimeOffset endsAt) =>
            startsAt >= endsAt;
    }
}
