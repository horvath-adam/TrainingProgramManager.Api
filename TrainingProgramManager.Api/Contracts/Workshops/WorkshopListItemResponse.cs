namespace TrainingProgramManager.Api.Contracts.Workshops
{
    public sealed record WorkshopListItemResponse(
        int Id,
        string Title,
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        string EventName,
        string RoomName,
        string SpeakerName);
}
