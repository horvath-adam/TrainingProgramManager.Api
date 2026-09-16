using Microsoft.AspNetCore.Mvc;

namespace TrainingProgramManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkshopsController : ControllerBase
    {
        public record WorkshopItem(int Id, string Title, string Category);

        private static readonly List<WorkshopItem> Workshops =
        [
            new WorkshopItem(1, "ASP.NET Core alapok", "backend"),
            new WorkshopItem(2, "EF Core bevezetés", "database"),
            new WorkshopItem(3, "JWT authentication alapok", "security")
        ];

        [HttpGet]
        public IActionResult Get([FromQuery] string? tag = null)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return Ok(Workshops);
            }

            var filtered = Workshops
                .Where(w => string.Equals(w.Category, tag, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(filtered);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var workshop = Workshops.FirstOrDefault(w => w.Id == id);

            if (workshop is null)
            {
                return NotFound();
            }

            return Ok(workshop);
        }
    }
}
