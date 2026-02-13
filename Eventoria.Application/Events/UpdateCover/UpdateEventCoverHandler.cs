using Eventoria.Application.Abstractions.Persistence;
using MediatR;

namespace Eventoria.Application.Events.UpdateCover;

public sealed class UpdateEventCoverHandler : IRequestHandler<UpdateEventCoverCommand, UpdateEventCoverResult>
{
    private readonly IEventRepository _events;
    private readonly IMediaFileRepository _mediaFiles;
    private readonly IUnitOfWork _uow;

    public UpdateEventCoverHandler(IEventRepository events, IMediaFileRepository mediaFiles, IUnitOfWork uow)
    {
        _events = events;
        _mediaFiles = mediaFiles;
        _uow = uow;
    }

    public async Task<UpdateEventCoverResult> Handle(UpdateEventCoverCommand cmd, CancellationToken ct)
    {
        if (cmd.ActorUserId == Guid.Empty) throw new InvalidOperationException("ActorUserId is required.");
        if (cmd.EventId == Guid.Empty) throw new InvalidOperationException("EventId is required.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            // 1) Check authorization: only event admins can update cover
            var isAdmin = await _events.IsEventAdminAsync(cmd.EventId, cmd.ActorUserId, innerCt);
            if (!isAdmin)
                throw new UnauthorizedAccessException("Only event admins can update event cover.");

            // 2) Get event
            var ev = await _events.GetByIdAsync(cmd.EventId, innerCt)
                ?? throw new InvalidOperationException("Event not found.");

            // 3) If a media file is provided, validate it exists and belongs to this event
            if (cmd.CoverPhotoMediaFileId.HasValue && cmd.CoverPhotoMediaFileId != Guid.Empty)
            {
                var mediaFile = await _mediaFiles.GetByIdAsync(cmd.CoverPhotoMediaFileId.Value, innerCt);
                if (mediaFile == null)
                    throw new InvalidOperationException("Media file not found.");

                if (mediaFile.EventId != cmd.EventId)
                    throw new InvalidOperationException("Media file does not belong to this event.");

                // Optionally: validate it's an image
                if (!mediaFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Cover photo must be an image.");
            }

            // 4) Update event cover photo
            ev.SetCoverPhoto(cmd.CoverPhotoMediaFileId);

            await _uow.SaveChangesAsync(innerCt);

            return new UpdateEventCoverResult(ev.Id);
        }, ct);
    }
}
