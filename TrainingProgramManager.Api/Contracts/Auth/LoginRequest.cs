using System.ComponentModel.DataAnnotations;

namespace TrainingProgramManager.Api.Contracts.Auth
{
    public sealed class LoginRequest
    {
        [Required(ErrorMessage = "Az e-mail cím kötelező.")]
        [EmailAddress(ErrorMessage = "Az e-mail cím formátuma érvénytelen.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó kötelező.")]
        public string Password { get; set; } = string.Empty;
    }
}
