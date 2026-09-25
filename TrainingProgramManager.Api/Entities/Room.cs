namespace TrainingProgramManager.Api.Entities
{
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
    }
}
