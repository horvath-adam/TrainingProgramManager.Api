namespace TrainingProgramManager.Api.Entities
{
    public class Registration
    {
        public int Id { get; set; }

        public int ParticipantId { get; set; }

        public Participant Participant { get; set; } = null!;

        public int WorkshopId { get; set; }

        public Workshop Workshop { get; set; } = null!;

        public DateTimeOffset RegisteredAt { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
