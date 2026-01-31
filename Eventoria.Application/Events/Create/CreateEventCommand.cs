using MediatR;

namespace Eventoria.Application.Events.Create;

public sealed record CreateEventCommand(
    Guid CreatorUserId,
    string Title,
    string? Description,
    DateOnly? Date,
    int ParticipantLimit,
    int PhotosPerUserLimit,
    int VideosPerUserLimit
) : IRequest<CreateEventResult>;
