using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingProgramManager.Api.Contracts.Workshops;
using TrainingProgramManager.Api.Data;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkshopsController : ControllerBase
    {
        private readonly TrainingProgramDbContext _dbContext;

        public WorkshopsController(TrainingProgramDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkshopResponse>>> GetAll([FromQuery] string? tag = null)
        {
            var query = _dbContext.Workshops.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(w => w.Tag.ToLower() == tag.ToLower());
            }

            var workshops = await query.ToListAsync();

            return Ok(workshops.Select(ToResponse));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkshopResponse>> GetById(int id)
        {
            var workshop = await _dbContext.Workshops.FirstOrDefaultAsync(w => w.Id == id);

            if (workshop is null)
            {
                return NotFound();
            }

            return Ok(ToResponse(workshop));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<WorkshopResponse>> Create([FromBody] CreateWorkshopRequest request)
        {
            if (string.Equals(request.Tag, "archived", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict("Archived tag cannot be used for new workshops.");
            }

            var workshop = new Workshop { Title = request.Title, Tag = request.Tag };

            _dbContext.Workshops.Add(workshop);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = workshop.Id }, ToResponse(workshop));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, UpdateWorkshopRequest request)
        {
            var workshop = await _dbContext.Workshops.FirstOrDefaultAsync(w => w.Id == id);

            if (workshop is null)
            {
                return NotFound();
            }

            workshop.Title = request.Title;
            workshop.Tag = request.Tag;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var workshop = await _dbContext.Workshops.FirstOrDefaultAsync(w => w.Id == id);

            if (workshop is null)
            {
                return NotFound();
            }

            _dbContext.Workshops.Remove(workshop);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private static WorkshopResponse ToResponse(Workshop workshop) =>
            new(workshop.Id, workshop.Title, workshop.Tag);
    }
}
