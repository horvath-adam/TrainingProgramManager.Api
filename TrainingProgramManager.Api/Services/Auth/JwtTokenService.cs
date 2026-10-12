using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TrainingProgramManager.Api.Contracts.Auth;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        // EN: Token creation is asynchronous because reading the user's roles from the database is asynchronous.
        // HU: A token létrehozása aszinkron, mert a felhasználó szerepköreinek adatbázisból történő olvasása aszinkron.
        public async Task<LoginResponse> CreateTokenAsync(ApplicationUser user)
        {
            var issuer = GetRequired("Jwt:Issuer");
            var audience = GetRequired("Jwt:Audience");
            var secretKey = GetRequired("Jwt:SecretKey");
            var expirationMinutes = int.Parse(GetRequired("Jwt:ExpirationMinutes"));

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

            // EN: Only identifying claims go into the token; never passwords, hashes or the whole user object.
            // HU: A token csak azonosító claim-eket tartalmaz; jelszót, hash-t vagy a teljes felhasználó objektumot soha.
            var claims = new List<Claim>
            {
                new("sub", user.Id),
                new("email", user.Email ?? string.Empty),
                new("name", user.DisplayName)
            };

            // EN: Identity stores role assignments separately from the user, so GetRolesAsync reads the user's current roles; a user without roles simply gets no role claims.
            // HU: Az Identity a szerepkör-hozzárendeléseket a felhasználótól külön tárolja, ezért a GetRolesAsync a felhasználó aktuális szerepköreit olvassa; szerepkör nélküli felhasználó egyszerűen nem kap role claim-et.
            var roles = await _userManager.GetRolesAsync(user);

            // EN: Each role becomes a ClaimTypes.Role claim, so authorization can evaluate the roles from the validated JWT without querying the database.
            // HU: Minden szerepkör egy ClaimTypes.Role claim lesz, így az engedélyezés a validált JWT-ből értékelheti a szerepköröket adatbázis-lekérdezés nélkül.
            // EN: The token only represents the roles at the time it was issued: changing a user's roles later does not modify an existing token; a new login issues a new token with the updated roles.
            // HU: A token csak a kiadás pillanatában érvényes szerepköröket tükrözi: a szerepkörök későbbi módosítása nem változtatja meg a már kiadott tokent; egy új bejelentkezés új, frissített szerepköröket tartalmazó tokent ad.
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials);

            return new LoginResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expiresAt
            };
        }

        private string GetRequired(string key) =>
            _configuration[key] ?? throw new InvalidOperationException($"Configuration value '{key}' is missing.");
    }
}
