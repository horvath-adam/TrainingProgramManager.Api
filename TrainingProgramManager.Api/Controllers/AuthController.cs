using Microsoft.AspNetCore.Mvc;
using TrainingProgramManager.Api.Contracts.Auth;
using TrainingProgramManager.Api.Services.Auth;

namespace TrainingProgramManager.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.IsSuccess)
            {
                // EN: Identity errors (e.g. duplicate email, weak password) are returned as a 400 ProblemDetails.
                // HU: Az Identity hibák (pl. foglalt e-mail, gyenge jelszó) 400-as ProblemDetails válaszként érkeznek vissza.
                return Problem(
                    title: "Registration failed",
                    detail: string.Join(" ", result.Errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return NoContent();
        }
    }
}
