using Microsoft.AspNetCore.Mvc;

namespace TrainingProgramManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkshopsController : ControllerBase
    {
        public record WorkshopItem(int Id, string Title, string Tag);

        public record CreateWorkshopRequest(string Title, string Tag);

        private static readonly List<WorkshopItem> Workshops =
        [
            new WorkshopItem(1, "ASP.NET Core alapok", "backend"),
            new WorkshopItem(2, "EF Core bevezetés", "database"),
            new WorkshopItem(3, "JWT authentication alapok", "security")
        ];

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll([FromQuery] string? tag = null)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return Ok(Workshops);
            }

            var filtered = Workshops
                .Where(w => string.Equals(w.Tag, tag, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(filtered);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var workshop = Workshops.FirstOrDefault(w => w.Id == id);

            if (workshop is null)
            {
                return NotFound();
            }

            return Ok(workshop);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult Create([FromBody] CreateWorkshopRequest request)
        {
            var nextId = Workshops.Count == 0 ? 1 : Workshops.Max(w => w.Id) + 1;
            var workshop = new WorkshopItem(nextId, request.Title, request.Tag);

            Workshops.Add(workshop);

            return CreatedAtAction(nameof(GetById), new { id = workshop.Id }, workshop);
        }
    }
}
