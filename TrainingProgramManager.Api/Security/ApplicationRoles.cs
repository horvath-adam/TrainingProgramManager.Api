namespace TrainingProgramManager.Api.Security
{
    public static class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string Organizer = "Organizer";
        public const string Participant = "Participant";
        public const string Speaker = "Speaker";

        public static readonly string[] All = [Admin, Organizer, Participant, Speaker];
    }
}
