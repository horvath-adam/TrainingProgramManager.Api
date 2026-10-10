namespace TrainingProgramManager.Api.Services.Auth
{
    public sealed class AuthResult
    {
        private AuthResult(bool isSuccess, IReadOnlyList<string> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public bool IsSuccess { get; }

        public IReadOnlyList<string> Errors { get; }

        public static AuthResult Success() => new(true, Array.Empty<string>());

        public static AuthResult Failure(IEnumerable<string> errors) => new(false, errors.ToList());
    }
}
