namespace Eventoria.Api.Contracts.Events;

public sealed record CreateEventRequestBody(
    string Title,
    string? Description,
    DateOnly? Date,
    int ParticipantLimit,
    int PhotosPerUserLimit,
    int VideosPerUserLimit
);
