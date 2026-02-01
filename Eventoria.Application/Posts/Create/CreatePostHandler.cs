using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
using MediatR;

namespace Eventoria.Application.Posts.Create;

public sealed class CreatePostHandler : IRequestHandler<CreatePostCommand, CreatePostResult>
{
    private readonly IPostRepository _posts;
    private readonly IEventRepository _events;
    private readonly IMediaFileRepository _mediaFiles;
    private readonly IUnitOfWork _uow;

    public CreatePostHandler(
        IPostRepository posts,
        IEventRepository events,
        IMediaFileRepository mediaFiles,
        IUnitOfWork uow)
    {
        _posts = posts;
        _events = events;
        _mediaFiles = mediaFiles;
        _uow = uow;
    }

    public async Task<CreatePostResult> Handle(CreatePostCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty) throw new InvalidOperationException("UserId required.");
        if (cmd.EventId == Guid.Empty) throw new InvalidOperationException("EventId required.");
        if (cmd.MediaFileIds == null || cmd.MediaFileIds.Count == 0)
            throw new InvalidOperationException("MediaFileIds is required.");

        // event membership check
        var isMember = await _events.IsMemberAsync(cmd.EventId, cmd.UserId, ct);
        if (!isMember) throw new UnauthorizedAccessException("You are not a member of this event.");

        // duplicates
        if (cmd.MediaFileIds.Distinct().Count() != cmd.MediaFileIds.Count)
            throw new InvalidOperationException("Duplicate mediaFileId in request.");

        // media existence + ownership
        var existsAll = await _mediaFiles.ExistsAllAsync(cmd.MediaFileIds, ct);
        if (!existsAll) throw new InvalidOperationException("Some media files not found.");

        var owned = await _mediaFiles.AreOwnedByUserAsync(cmd.UserId, cmd.MediaFileIds, ct);
        if (!owned) throw new UnauthorizedAccessException("You can only use your own uploaded media.");

        var post = new Post(cmd.EventId, cmd.UserId, cmd.Caption);

        foreach (var id in cmd.MediaFileIds)
            post.AddMedia(id);

        await _posts.AddAsync(post, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreatePostResult(post.Id);
    }
}
