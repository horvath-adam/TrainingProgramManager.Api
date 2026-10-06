using Microsoft.AspNetCore.Mvc;
using TrainingProgramManager.Api.Contracts.Workshops;
using TrainingProgramManager.Api.Services.Workshops;

namespace TrainingProgramManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkshopsController : ControllerBase
    {
        private readonly IWorkshopService _workshopService;

        public WorkshopsController(IWorkshopService workshopService)
        {
            _workshopService = workshopService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkshopListItemResponse>>> GetAll([FromQuery] int? eventId = null)
        {
            var workshops = await _workshopService.GetAllAsync(eventId);

            return Ok(workshops);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkshopDetailsResponse>> GetById(int id)
        {
            var workshop = await _workshopService.GetByIdAsync(id);

            if (workshop is null)
            {
                return WorkshopNotFound();
            }

            return Ok(workshop);
        }

        [HttpGet("{id:int}/tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<string>>> GetTags(int id)
        {
            var tagNames = await _workshopService.GetTagsAsync(id);

            if (tagNames is null)
            {
                return WorkshopNotFound();
            }

            return Ok(tagNames);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<WorkshopDetailsResponse>> Create([FromBody] CreateWorkshopRequest request)
        {
            var result = await _workshopService.CreateAsync(request);

            if (!result.IsSuccess)
            {
                return ToProblem(result.ErrorType, result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, UpdateWorkshopRequest request)
        {
            var result = await _workshopService.UpdateAsync(id, request);

            if (!result.IsSuccess)
            {
                return ToProblem(result.ErrorType, result.ErrorMessage);
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _workshopService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                return ToProblem(result.ErrorType, result.ErrorMessage);
            }

            return NoContent();
        }

        // EN: Single place that maps a typed service error to a ProblemDetails response.
        // HU: Az egyetlen hely, ahol a típusos service hiba ProblemDetails válaszra képeződik le.
        private ObjectResult ToProblem(ServiceErrorType errorType, string? errorMessage)
        {
            var statusCode = errorType switch
            {
                ServiceErrorType.Validation => StatusCodes.Status400BadRequest,
                ServiceErrorType.NotFound => StatusCodes.Status404NotFound,
                ServiceErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            return Problem(detail: errorMessage, statusCode: statusCode);
        }

        private ObjectResult WorkshopNotFound() =>
            ToProblem(ServiceErrorType.NotFound, "Workshop not found.");
    }
}
