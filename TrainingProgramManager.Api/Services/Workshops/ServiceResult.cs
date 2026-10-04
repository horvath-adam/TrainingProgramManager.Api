namespace TrainingProgramManager.Api.Services.Workshops
{
    public sealed class ServiceResult
    {
        private ServiceResult(bool isSuccess, string? errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public string? ErrorMessage { get; }

        public static ServiceResult Success() => new(true, null);

        public static ServiceResult Failure(string errorMessage) => new(false, errorMessage);
    }

    public sealed class ServiceResult<T>
    {
        private ServiceResult(bool isSuccess, T? value, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public T? Value { get; }

        public string? ErrorMessage { get; }

        public static ServiceResult<T> Success(T value) => new(true, value, null);

        public static ServiceResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
    }
}
