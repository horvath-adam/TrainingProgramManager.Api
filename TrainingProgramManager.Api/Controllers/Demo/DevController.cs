using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrainingProgramManager.Api.Entities;
using TrainingProgramManager.Api.Security;

namespace TrainingProgramManager.Api.Controllers.Demo
{
    // EN: Educational demo only: this controller exists exclusively to support the authorization demonstration and is not part of the TrainingProgramManager business requirements.
    // HU: Kizárólag oktatási demó: ez a controller kizárólag az engedélyezési bemutatót támogatja, és nem része a TrainingProgramManager üzleti követelményeinek.
    // EN: It is NOT a production administration API. It is anonymous and therefore security-sensitive: it must never assign a role outside the Development environment.
    // HU: NEM éles adminisztrációs API. Névtelen, ezért biztonsági szempontból érzékeny: Development környezeten kívül sosem rendelhet hozzá szerepkört.
    // EN: A Development environment may also be reachable over the network, so the environment check alone does not make this endpoint safe for publicly accessible deployments.
    // HU: Egy Development környezet is lehet hálózaton keresztül elérhető, ezért a környezet-ellenőrzés önmagában nem teszi biztonságossá ezt az endpointot nyilvánosan elérhető telepítéseknél.
    [ApiController]
    [Route("api/dev")]
    public class DevController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public DevController(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        // EN: Educational shortcut only, NOT a production administration API: it is anonymous and lets anyone make a user an Organizer, so it works only in the Development environment.
        // HU: Kizárólag oktatási célú rövidítés, NEM éles adminisztrációs API: névtelen, és bárkit Organizerré tehet, ezért csak a Development környezetben működik.
        // EN: Changing a role assignment does not rewrite an already issued JWT; the user must log in again to get a token with the updated role claims.
        // HU: A szerepkör-hozzárendelés módosítása nem írja át a már kiadott JWT-t; a frissített szerepkör claim-ekhez a felhasználónak újra be kell jelentkeznie.
        [AllowAnonymous]
        [HttpPost("make-organizer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MakeOrganizer([FromQuery] string? email)
        {
            // EN: Guard: outside Development return 404 before any Identity operation, so privileges are never elevated and user existence is not revealed.
            // HU: Védelem: Development környezeten kívül 404-et adunk bármilyen Identity művelet előtt, így jogosultság sosem emelkedik, és a felhasználó létezése sem derül ki.
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            // EN: The parameter is nullable and validated here, after the guard: a non-nullable [FromQuery] string would make [ApiController] return 400 for a missing email before the action (and its Development guard) runs.
            // HU: A paraméter nullable, és az ellenőrzés itt történik, a védelem után: egy nem nullable [FromQuery] string esetén az [ApiController] hiányzó e-mailnél 400-at adna még az action (és a Development védelem) lefutása előtt.
            if (string.IsNullOrWhiteSpace(email))
            {
                return Problem(
                    title: "Email is required",
                    detail: "The 'email' query parameter is required.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return NotFound();
            }

            // EN: Idempotent: a user who is already an Organizer is not assigned the role again. Other roles, such as Participant, are left untouched.
            // HU: Idempotens: aki már Organizer, annak nem rendeljük hozzá újra a szerepkört. A többi szerepkör, például a Participant, érintetlen marad.
            if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Organizer))
            {
                return NoContent();
            }

            var result = await _userManager.AddToRoleAsync(user, ApplicationRoles.Organizer);

            if (!result.Succeeded)
            {
                return Problem(
                    title: "Role assignment failed",
                    detail: string.Join(" ", result.Errors.Select(e => e.Description)),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return NoContent();
        }
    }
}
