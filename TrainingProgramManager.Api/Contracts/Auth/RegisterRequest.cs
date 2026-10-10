using System.ComponentModel.DataAnnotations;

namespace TrainingProgramManager.Api.Contracts.Auth
{
    public sealed class RegisterRequest
    {
        [Required(ErrorMessage = "Az e-mail cím kötelező.")]
        [EmailAddress(ErrorMessage = "Az e-mail cím formátuma érvénytelen.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó kötelező.")]
        [MinLength(8, ErrorMessage = "A jelszó legalább 8 karakter hosszú legyen.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "A megjelenített név kötelező.")]
        [StringLength(100, ErrorMessage = "A megjelenített név legfeljebb 100 karakter lehet.")]
        public string DisplayName { get; set; } = string.Empty;
    }
}
