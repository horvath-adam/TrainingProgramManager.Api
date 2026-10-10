using TrainingProgramManager.Api.Contracts.Auth;

namespace TrainingProgramManager.Api.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
    }
}
