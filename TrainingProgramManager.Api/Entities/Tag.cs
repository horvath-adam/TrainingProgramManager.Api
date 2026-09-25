namespace TrainingProgramManager.Api.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
    }
}
