using System.IdentityModel.Tokens.Jwt;

namespace TrainingProgramManager.Api.Security
{
    // EN: Application services depend on this interface instead of HttpContext, so they stay independent of HTTP details and easy to read and test.
    // HU: Az alkalmazás service-ei HttpContext helyett ettől az interfésztől függenek, így függetlenek maradnak a HTTP részletektől, és könnyebben olvashatók és tesztelhetők.
    public interface ICurrentUserService
    {
        string GetRequiredUserId();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetRequiredUserId()
        {
            // EN: The id comes from the validated JWT, never from client-controlled input such as the body, route or query string, so a caller cannot claim to be someone else.
            // HU: Az azonosító a validált JWT-ből jön, soha nem kliens által irányított bemenetből (body, route, query string), így a hívó nem adhatja ki magát másnak.
            // EN: The "sub" claim is read directly because MapInboundClaims = false keeps the original JWT claim names, so ClaimTypes.NameIdentifier is not populated.
            // HU: A "sub" claim-et közvetlenül olvassuk, mert a MapInboundClaims = false megtartja az eredeti JWT claim neveket, így a ClaimTypes.NameIdentifier nincs kitöltve.
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new InvalidOperationException("The current user id ('sub' claim) is missing from the authenticated principal.");
            }

            return userId;
        }
    }
}
