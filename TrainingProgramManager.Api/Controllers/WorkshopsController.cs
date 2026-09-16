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
        public IActionResult Get()
        {
            return Ok(Workshops);
        }
    }
}
