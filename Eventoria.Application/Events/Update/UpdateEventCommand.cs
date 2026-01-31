using MediatR;

namespace Eventoria.Application.Events.Update;

public sealed record UpdateEventCommand(
    Guid ActorUserId,
    Guid EventId,
    string Title,
    string? Description,
    DateOnly? Date,
    int ParticipantLimit,
    int PhotosPerUserLimit,
    int VideosPerUserLimit
) : IRequest<UpdateEventResult>;
