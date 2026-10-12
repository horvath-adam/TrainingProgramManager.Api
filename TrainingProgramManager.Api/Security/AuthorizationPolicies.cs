namespace TrainingProgramManager.Api.Security
{
    public static class AuthorizationPolicies
    {
        public const string OrganizerOnly = "OrganizerOnly";
        public const string EventManagement = "EventManagement";
        public const string VerifiedEmail = "VerifiedEmail";
    }
}
