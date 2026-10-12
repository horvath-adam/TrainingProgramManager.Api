using TrainingProgramManager.Api.Contracts.Auth;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Auth
{
    public interface IJwtTokenService
    {
        Task<LoginResponse> CreateTokenAsync(ApplicationUser user);
    }
}
