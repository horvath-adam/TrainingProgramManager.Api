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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkshopDetailsResponse>> GetById(int id)
        {
            var workshop = await _workshopService.GetByIdAsync(id);

            if (workshop is null)
            {
                return NotFound();
            }

            return Ok(workshop);
        }

        [HttpGet("{id:int}/tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<string>>> GetTags(int id)
        {
            var tagNames = await _workshopService.GetTagsAsync(id);

            if (tagNames is null)
            {
                return NotFound();
            }

            return Ok(tagNames);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WorkshopDetailsResponse>> Create([FromBody] CreateWorkshopRequest request)
        {
            var result = await _workshopService.CreateAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, UpdateWorkshopRequest request)
        {
            var result = await _workshopService.UpdateAsync(id, request);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _workshopService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage);
            }

            return NoContent();
        }
    }
}
