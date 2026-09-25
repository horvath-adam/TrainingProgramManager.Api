namespace TrainingProgramManager.Api.Contracts.Workshops
{
    public sealed record WorkshopResponse(int Id, string Title, int EventId, int RoomId, int SpeakerId);
}
