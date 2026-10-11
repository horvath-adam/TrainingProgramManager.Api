namespace TrainingProgramManager.Api.Services.Auth
{
    public sealed class AuthResult<T>
    {
        private AuthResult(bool isSuccess, T? value, IReadOnlyList<string> errors)
        {
            IsSuccess = isSuccess;
            Value = value;
            Errors = errors;
        }

        public bool IsSuccess { get; }

        public T? Value { get; }

        public IReadOnlyList<string> Errors { get; }

        public static AuthResult<T> Success(T value) => new(true, value, Array.Empty<string>());

        public static AuthResult<T> Failure(IEnumerable<string> errors) => new(false, default, errors.ToList());
    }
}
