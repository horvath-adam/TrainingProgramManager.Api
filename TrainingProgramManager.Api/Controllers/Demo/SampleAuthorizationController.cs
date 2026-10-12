using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingProgramManager.Api.Security;

namespace TrainingProgramManager.Api.Controllers.Demo
{
    // EN: Educational demo only: this controller exists exclusively to demonstrate authorization (authentication, roles, policies and claims) and to help students understand HTTP 401 versus HTTP 403.
    // HU: Kizárólag oktatási demó: ez a controller kizárólag az engedélyezés (hitelesítés, szerepkörök, policy-k és claim-ek) bemutatására szolgál, és segít megérteni a HTTP 401 és a HTTP 403 különbségét.
    // EN: It is not part of the TrainingProgramManager business requirements and implements no domain-specific business operations.
    // HU: Nem része a TrainingProgramManager üzleti követelményeinek, és nem valósít meg domain-specifikus üzleti műveleteket.
    // EN: Demo endpoints for authorization. No token gives 401 Unauthorized (authentication failed); an authenticated caller who lacks the required role, policy or claim gets 403 Forbidden (authorization failed).
    // HU: Demó endpointok az engedélyezéshez. Token nélkül 401 Unauthorized (a hitelesítés sikertelen); a hitelesített hívó, akinek nincs meg a szükséges szerepköre, policy-ja vagy claim-je, 403 Forbidden választ kap (az engedélyezés sikertelen).
    [ApiController]
    [Route("api/sample-authorization")]
    public class SampleAuthorizationController : ControllerBase
    {
        // EN: [Authorize] alone requires authentication but no particular role.
        // HU: Az önmagában álló [Authorize] hitelesítést igényel, de konkrét szerepkört nem.
        [Authorize]
        [HttpGet("authenticated")]
        public IActionResult Authenticated()
        {
            return Ok(new { Message = "Any authenticated user can access this endpoint." });
        }

        // EN: [Authorize(Roles = ...)] checks the role claims carried by the validated JWT.
        // HU: Az [Authorize(Roles = ...)] a validált JWT-ben szereplő szerepkör claim-eket ellenőrzi.
        [Authorize(Roles = ApplicationRoles.Organizer)]
        [HttpGet("organizer-role")]
        public IActionResult OrganizerRole()
        {
            return Ok(new { Message = "Only Organizer role can access this endpoint." });
        }

        // EN: A comma-separated role list in one attribute means OR: Admin alone or Organizer alone is enough.
        // HU: Egyetlen attribútumban a vesszővel elválasztott szerepkörlista VAGY kapcsolatot jelent: elég az Admin vagy elég az Organizer.
        [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.Organizer)]
        [HttpGet("admin-or-organizer-role")]
        public IActionResult AdminOrOrganizerRole()
        {
            return Ok(new { Message = "Admin or Organizer role can access this endpoint." });
        }

        // EN: [Authorize(Policy = ...)] references a rule registered centrally in Program.cs instead of repeating it here.
        // HU: Az [Authorize(Policy = ...)] a Program.cs-ben központilag regisztrált szabályra hivatkozik, nem ismétli meg azt itt.
        [Authorize(Policy = AuthorizationPolicies.OrganizerOnly)]
        [HttpGet("organizer-policy")]
        public IActionResult OrganizerPolicy()
        {
            return Ok(new { Message = "OrganizerOnly policy succeeded." });
        }

        [Authorize(Policy = AuthorizationPolicies.EventManagement)]
        [HttpGet("event-management-policy")]
        public IActionResult EventManagementPolicy()
        {
            return Ok(new { Message = "EventManagement policy succeeded." });
        }

        // EN: VerifiedEmail demonstrates claim-based (not role-based) authorization; the tokens do not carry email_verified, so an authenticated caller gets 403 here by design.
        // HU: A VerifiedEmail claim alapú (nem szerepkör alapú) engedélyezést mutat be; a tokenek nem tartalmaznak email_verified claim-et, ezért a hitelesített hívó itt szándékosan 403-at kap.
        [Authorize(Policy = AuthorizationPolicies.VerifiedEmail)]
        [HttpGet("verified-email-policy")]
        public IActionResult VerifiedEmailPolicy()
        {
            return Ok(new { Message = "VerifiedEmail policy succeeded." });
        }
    }
}
