namespace TrainingProgramManager.Api.Entities
{
    public class Speaker
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
    }
}
