using Microsoft.AspNetCore.Mvc;

namespace TrainingProgramManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkshopsController : ControllerBase
    {
        private static readonly List<WorkshopItem> Workshops =
        [
            new WorkshopItem(1, "ASP.NET Core alapok", "backend"),
            new WorkshopItem(2, "EF Core bevezetés", "database"),
            new WorkshopItem(3, "JWT authentication alapok", "security")
        ];

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<WorkshopResponse>> GetAll([FromQuery] string? tag = null)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return Ok(Workshops.Select(ToResponse));
            }

            var filtered = Workshops
                .Where(w => string.Equals(w.Tag, tag, StringComparison.OrdinalIgnoreCase))
                .Select(ToResponse)
                .ToList();

            return Ok(filtered);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<WorkshopResponse> GetById(int id)
        {
            var workshop = Workshops.FirstOrDefault(w => w.Id == id);

            if (workshop is null)
            {
                return NotFound();
            }

            return Ok(ToResponse(workshop));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult<WorkshopResponse> Create([FromBody] CreateWorkshopRequest request)
        {
            var nextId = Workshops.Count == 0 ? 1 : Workshops.Max(w => w.Id) + 1;
            var workshop = new WorkshopItem(nextId, request.Title, request.Tag);

            Workshops.Add(workshop);

            return CreatedAtAction(nameof(GetById), new { id = workshop.Id }, ToResponse(workshop));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(int id, UpdateWorkshopRequest request)
        {
            var index = Workshops.FindIndex(w => w.Id == id);

            if (index == -1)
            {
                return NotFound();
            }

            Workshops[index] = Workshops[index] with { Title = request.Title, Tag = request.Tag };

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var workshop = Workshops.FirstOrDefault(w => w.Id == id);

            if (workshop is null)
            {
                return NotFound();
            }

            Workshops.Remove(workshop);

            return NoContent();
        }

        private static WorkshopResponse ToResponse(WorkshopItem workshop) =>
            new(workshop.Id, workshop.Title, workshop.Tag);

        private sealed record WorkshopItem(int Id, string Title, string Tag);

        public sealed record CreateWorkshopRequest(string Title, string Tag);

        public sealed record UpdateWorkshopRequest(string Title, string Tag);

        public sealed record WorkshopResponse(int Id, string Title, string Tag);
    }
}
