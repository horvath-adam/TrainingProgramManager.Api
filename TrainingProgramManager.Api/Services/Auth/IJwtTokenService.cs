using TrainingProgramManager.Api.Contracts.Auth;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Auth
{
    public interface IJwtTokenService
    {
        LoginResponse CreateToken(ApplicationUser user);
    }
}
