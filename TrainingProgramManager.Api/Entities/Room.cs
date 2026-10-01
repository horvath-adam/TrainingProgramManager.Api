namespace TrainingProgramManager.Api.Entities
{
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public string Building { get; set; } = string.Empty;

        public ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
    }
}
