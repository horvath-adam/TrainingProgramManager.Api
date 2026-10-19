using Microsoft.AspNetCore.Identity;
using TrainingProgramManager.Api.Contracts.Auth;
using TrainingProgramManager.Api.Data;
using TrainingProgramManager.Api.Entities;
using TrainingProgramManager.Api.Security;

namespace TrainingProgramManager.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly TrainingProgramDbContext _dbContext;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            TrainingProgramDbContext dbContext,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName
            };

            // EN: Creating the user and assigning the default Participant role form ONE logical operation: a user without a role would be a half-finished registration.
            // HU: A felhasználó létrehozása és az alapértelmezett Participant szerepkör kiosztása EGY logikai művelet: a szerepkör nélküli felhasználó félkész regisztráció lenne.
            // EN: A database transaction means that either both database changes are committed, or neither of them remains in the database.
            // HU: Az adatbázis-tranzakció azt jelenti, hogy vagy mindkét adatbázis-módosítás véglegesül, vagy egyik sem marad az adatbázisban.
            // EN: The two UserManager calls need an explicit transaction because Identity saves its changes during each call separately; without it the user could be saved while the role assignment fails.
            // HU: A két UserManager hívást explicit tranzakcióval kell védeni, mert az Identity minden hívás során külön menti a módosításait; enélkül a felhasználó elmentődhetne, miközben a szerepkör kiosztása sikertelen.
            // EN: UserManager uses the same scoped TrainingProgramDbContext as this service, so the transaction started on it covers both Identity operations.
            // HU: A UserManager ugyanazt a scope-olt TrainingProgramDbContext-et használja, mint ez a service, ezért az azon indított tranzakció mindkét Identity műveletet lefedi.
            // EN: "await using" disposes the transaction when the method ends; a transaction that was not committed is rolled back on disposal, even if an exception is thrown.
            // HU: Az "await using" a metódus végén felszabadítja a tranzakciót; a nem véglegesített tranzakció felszabadításkor visszagördül, kivétel esetén is.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // EN: The request carries the plaintext password; UserManager hashes it securely, and the application never stores the plaintext.
            // HU: A kérés nyers szöveges jelszót tartalmaz; a UserManager biztonságosan hash-eli, az alkalmazás a nyers jelszót nem tárolja.
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                // EN: Rollback is required because the IdentityResult reports a failure: nothing from this registration may stay in the database. No role is assigned.
                // HU: A visszagörgetés azért szükséges, mert az IdentityResult hibát jelez: ebből a regisztrációból semmi sem maradhat az adatbázisban. Szerepkör kiosztására nem kerül sor.
                await transaction.RollbackAsync();

                return AuthResult.Failure(result.Errors.Select(e => e.Description));
            }

            // EN: Every self-registered user gets the least-privileged Participant role; privileged roles are never assigned here.
            // HU: Minden önállóan regisztrált felhasználó a legkisebb jogosultságú Participant szerepkört kapja; privilegizált szerepkört itt soha nem rendelünk hozzá.
            var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.Participant);

            if (!roleResult.Succeeded)
            {
                // EN: Rollback is required here too: it also removes the user that was already saved by CreateAsync, so no user is left without a role.
                // HU: Itt is szükséges a visszagörgetés: ez a CreateAsync által már elmentett felhasználót is eltávolítja, így nem marad szerepkör nélküli felhasználó.
                await transaction.RollbackAsync();

                return AuthResult.Failure(roleResult.Errors.Select(e => e.Description));
            }

            // EN: CommitAsync runs only after BOTH the user creation AND the role assignment succeeded; only then do the changes become permanent. If it throws, the exception propagates and success is never reported.
            // HU: A CommitAsync csak akkor fut le, ha a felhasználó létrehozása ÉS a szerepkör kiosztása is sikerült; a módosítások csak ekkor válnak véglegessé. Ha kivételt dob, az továbbterjed, és sikert sosem jelzünk.
            await transaction.CommitAsync();

            _logger.LogInformation("User {UserId} registered.", user.Id);

            return AuthResult.Success();
        }

        public async Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            // EN: Unknown email and wrong password return the same message so the API does not reveal which emails are registered.
            // HU: Ismeretlen e-mail és hibás jelszó ugyanazt az üzenetet adja, így az API nem árulja el, mely e-mail címek regisztráltak.
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Failed login attempt.");

                return AuthResult<LoginResponse>.Failure(new[] { "Invalid email or password." });
            }

            var response = await _jwtTokenService.CreateTokenAsync(user);

            _logger.LogInformation("User {UserId} logged in successfully.", user.Id);

            return AuthResult<LoginResponse>.Success(response);
        }
    }
}
