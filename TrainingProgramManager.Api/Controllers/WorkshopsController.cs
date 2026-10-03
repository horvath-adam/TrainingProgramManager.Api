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
        public async Task<ActionResult<IEnumerable<WorkshopListItemResponse>>> GetAll([FromQuery] int? eventId = null)
        {
            // EN: AsNoTracking() states the read intent; the DTO projection itself does not create tracked entities.
            // HU: Az AsNoTracking() jelzi az olvasási szándékot; a DTO projekció önmagában nem hoz létre követett entitásokat.
            var query = _dbContext.Workshops.AsNoTracking();

            if (eventId is not null)
            {
                query = query.Where(w => w.EventId == eventId);
            }

            var workshops = await query
                .Select(w => new WorkshopListItemResponse(
                    w.Id,
                    w.Title,
                    w.StartsAt,
                    w.EndsAt,
                    w.Event.Name,
                    w.Room.Name,
                    w.Speaker.FullName))
                .ToListAsync();

            // EN: The SQLite provider cannot translate ORDER BY on DateTimeOffset, so the small projected result is ordered in memory.
            // HU: Az SQLite provider nem tudja lefordítani a DateTimeOffset szerinti ORDER BY-t, ezért a kis méretű projektált eredményt memóriában rendezzük.
            return Ok(workshops.OrderBy(w => w.StartsAt).ThenBy(w => w.Id));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkshopDetailsResponse>> GetById(int id)
        {
            var workshop = await _dbContext.Workshops
                .AsNoTracking()
                .Where(w => w.Id == id)
                .Select(w => new WorkshopDetailsResponse(
                    w.Id,
                    w.Title,
                    w.StartsAt,
                    w.EndsAt,
                    w.EventId,
                    w.Event.Name,
                    w.RoomId,
                    w.Room.Name,
                    w.SpeakerId,
                    w.Speaker.FullName,
                    w.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList()))
                .FirstOrDefaultAsync();

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
            var workshopExists = await _dbContext.Workshops.AnyAsync(w => w.Id == id);

            if (!workshopExists)
            {
                return NotFound();
            }

            var tagNames = await _dbContext.Workshops
                .AsNoTracking()
                .Where(w => w.Id == id)
                .SelectMany(w => w.Tags)
                .Select(t => t.Name)
                .OrderBy(name => name)
                .ToListAsync();

            return Ok(tagNames);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WorkshopResponse>> Create([FromBody] CreateWorkshopRequest request)
        {
            if (!await ReferencedEntitiesExistAsync(request.EventId, request.RoomId, request.SpeakerId))
            {
                return BadRequest("Az esemény, terem vagy előadó azonosítója érvénytelen.");
            }

            var workshop = new Workshop
            {
                Title = request.Title,
                EventId = request.EventId,
                RoomId = request.RoomId,
                SpeakerId = request.SpeakerId,
                StartsAt = DateTimeOffset.UtcNow,
                EndsAt = DateTimeOffset.UtcNow.AddHours(2)
            };

            _dbContext.Workshops.Add(workshop);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = workshop.Id }, ToResponse(workshop));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, UpdateWorkshopRequest request)
        {
            var workshop = await _dbContext.Workshops.FirstOrDefaultAsync(w => w.Id == id);

            if (workshop is null)
            {
                return NotFound();
            }

            if (!await ReferencedEntitiesExistAsync(request.EventId, request.RoomId, request.SpeakerId))
            {
                return BadRequest("Az esemény, terem vagy előadó azonosítója érvénytelen.");
            }

            workshop.Title = request.Title;
            workshop.EventId = request.EventId;
            workshop.RoomId = request.RoomId;
            workshop.SpeakerId = request.SpeakerId;

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

        private async Task<bool> ReferencedEntitiesExistAsync(int eventId, int roomId, int speakerId)
        {
            var eventExists = await _dbContext.Events.AnyAsync(e => e.Id == eventId);
            var roomExists = await _dbContext.Rooms.AnyAsync(r => r.Id == roomId);
            var speakerExists = await _dbContext.Speakers.AnyAsync(s => s.Id == speakerId);

            return eventExists && roomExists && speakerExists;
        }

        private static WorkshopResponse ToResponse(Workshop workshop) =>
            new(workshop.Id, workshop.Title, workshop.EventId, workshop.RoomId, workshop.SpeakerId);
    }
}
