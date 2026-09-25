namespace TrainingProgramManager.Api.Entities
{
    public class Participant
    {
        public int Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
