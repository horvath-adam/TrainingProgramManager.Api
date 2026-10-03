namespace TrainingProgramManager.Api.Contracts.Workshops
{
    public sealed record WorkshopDetailsResponse(
        int Id,
        string Title,
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        int EventId,
        string EventName,
        int RoomId,
        string RoomName,
        int SpeakerId,
        string SpeakerName,
        IReadOnlyCollection<string> Tags);
}
