using System.ComponentModel.DataAnnotations;

namespace TrainingProgramManager.Api.Contracts.Workshops
{
    public sealed class UpdateWorkshopRequest
    {
        [Required(ErrorMessage = "A workshop címe kötelező.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "A workshop címe 3 és 100 karakter között legyen.")]
        public string Title { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Az esemény azonosítója érvényes kell legyen.")]
        public int EventId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A terem azonosítója érvényes kell legyen.")]
        public int RoomId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Az előadó azonosítója érvényes kell legyen.")]
        public int SpeakerId { get; set; }
    }
}
