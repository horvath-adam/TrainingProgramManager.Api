namespace TrainingProgramManager.Api.Services.Workshops
{
    public sealed class ServiceResult
    {
        private ServiceResult(bool isSuccess, ServiceErrorType errorType, string? errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorType = errorType;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public ServiceErrorType ErrorType { get; }

        public string? ErrorMessage { get; }

        public static ServiceResult Success() => new(true, ServiceErrorType.None, null);

        public static ServiceResult Failure(ServiceErrorType errorType, string errorMessage) =>
            new(false, errorType, errorMessage);
    }

    public sealed class ServiceResult<T>
    {
        private ServiceResult(bool isSuccess, T? value, ServiceErrorType errorType, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorType = errorType;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public T? Value { get; }

        public ServiceErrorType ErrorType { get; }

        public string? ErrorMessage { get; }

        public static ServiceResult<T> Success(T value) => new(true, value, ServiceErrorType.None, null);

        public static ServiceResult<T> Failure(ServiceErrorType errorType, string errorMessage) =>
            new(false, default, errorType, errorMessage);
    }
}
