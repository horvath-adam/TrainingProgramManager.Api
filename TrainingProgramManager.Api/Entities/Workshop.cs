namespace TrainingProgramManager.Api.Entities
{
    public class Workshop
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTimeOffset StartsAt { get; set; }

        public DateTimeOffset EndsAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = null!;

        public int RoomId { get; set; }

        public Room Room { get; set; } = null!;

        public int SpeakerId { get; set; }

        public Speaker Speaker { get; set; } = null!;

        // EN: Every workshop must have exactly one owner (an Identity user); the owner is always set by the server, never by the client.
        // HU: Minden workshopnak pontosan egy tulajdonosa (Identity felhasználója) kell legyen; a tulajdonost mindig a szerver állítja be, soha nem a kliens.
        public required string OrganizerId { get; set; }

        public ApplicationUser Organizer { get; set; } = null!;

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
