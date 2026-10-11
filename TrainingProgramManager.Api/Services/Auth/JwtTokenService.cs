using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TrainingProgramManager.Api.Contracts.Auth;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Services.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoginResponse CreateToken(ApplicationUser user)
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
