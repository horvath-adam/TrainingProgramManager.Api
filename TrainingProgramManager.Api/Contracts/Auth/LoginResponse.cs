namespace TrainingProgramManager.Api.Contracts.Auth
{
    public sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }
    }
}
