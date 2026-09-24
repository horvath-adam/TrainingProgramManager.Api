using System.ComponentModel.DataAnnotations;

namespace TrainingProgramManager.Api.Contracts.Workshops
{
    public sealed class UpdateWorkshopRequest
    {
        [Required(ErrorMessage = "A workshop címe kötelező.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "A workshop címe 3 és 100 karakter között legyen.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "A címke megadása kötelező.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "A címke 2 és 30 karakter között legyen.")]
        public string Tag { get; set; } = string.Empty;
    }
}
