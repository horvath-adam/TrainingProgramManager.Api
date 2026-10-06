namespace TrainingProgramManager.Api.Services.Workshops
{
    // EN: Describes why a service operation failed, without any HTTP-specific concept.
    // HU: Leírja, miért hiúsult meg a service művelet, HTTP-specifikus fogalom nélkül.
    public enum ServiceErrorType
    {
        None,
        Validation,
        NotFound,
        Conflict
    }
}
