using Microsoft.AspNetCore.Identity;
using TrainingProgramManager.Api.Contracts.Auth;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
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

        public async Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            // EN: Unknown email and wrong password return the same message so the API does not reveal which emails are registered.
            // HU: Ismeretlen e-mail és hibás jelszó ugyanazt az üzenetet adja, így az API nem árulja el, mely e-mail címek regisztráltak.
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return AuthResult<LoginResponse>.Failure(new[] { "Invalid email or password." });
            }

            return AuthResult<LoginResponse>.Success(_jwtTokenService.CreateToken(user));
        }
    }
}
