using MediatR;

namespace Eventoria.Application.Events.UpdateCover;

public sealed record UpdateEventCoverCommand(
    Guid ActorUserId,
    Guid EventId,
    Guid? CoverPhotoMediaFileId) : IRequest<UpdateEventCoverResult>;
