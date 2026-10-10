using Microsoft.AspNetCore.Identity;

namespace TrainingProgramManager.Api.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
