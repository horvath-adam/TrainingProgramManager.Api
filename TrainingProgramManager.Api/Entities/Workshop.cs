namespace TrainingProgramManager.Api.Entities
{
    public class Workshop
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTimeOffset StartsAt { get; set; }

        public DateTimeOffset EndsAt { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = null!;

        public int RoomId { get; set; }

        public Room Room { get; set; } = null!;

        public int SpeakerId { get; set; }

        public Speaker Speaker { get; set; } = null!;

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
