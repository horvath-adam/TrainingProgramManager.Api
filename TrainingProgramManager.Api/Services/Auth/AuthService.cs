using Microsoft.AspNetCore.Identity;
using TrainingProgramManager.Api.Contracts.Auth;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName
            };

            // EN: The request carries the plaintext password; UserManager hashes it securely, and the application never stores the plaintext.
            // HU: A kérés nyers szöveges jelszót tartalmaz; a UserManager biztonságosan hash-eli, az alkalmazás a nyers jelszót nem tárolja.
            var result = await _userManager.CreateAsync(user, request.Password);

            return result.Succeeded
                ? AuthResult.Success()
                : AuthResult.Failure(result.Errors.Select(e => e.Description));
        }
    }
}
