using Eventoria.Application.Abstractions.Persistence;
using MediatR;

namespace Eventoria.Application.Posts.Media.Remove;

public sealed class RemovePostMediaHandler : IRequestHandler<RemovePostMediaCommand, RemovePostMediaResult>
{
    private readonly IPostRepository _posts;
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;

    public RemovePostMediaHandler(IPostRepository posts, IEventRepository events, IUnitOfWork uow)
    {
        _posts = posts;
        _events = events;
        _uow = uow;
    }

    public async Task<RemovePostMediaResult> Handle(RemovePostMediaCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (cmd.PostId == Guid.Empty) throw new InvalidOperationException("PostId is required.");
        if (cmd.MediaFileId == Guid.Empty) throw new InvalidOperationException("MediaFileId is required.");

        // Media listesi lazım
        var post = await _posts.GetByIdWithMediaAsync(cmd.PostId, ct)
            ?? throw new InvalidOperationException("Post not found.");

        var isOwner = post.CreatedByUserId == cmd.UserId;
        var isAdmin = await _events.IsEventAdminAsync(post.EventId, cmd.UserId, ct);

        if (!isOwner && !isAdmin)
            throw new UnauthorizedAccessException("Not allowed.");

        // PostMedia kaldır
        var removed = post.RemoveMedia(cmd.MediaFileId);
        if (!removed)
            throw new InvalidOperationException("Media not found in post.");

        // Order'ları sıkıştır (0..n-1)
        post.NormalizeOrder();

        _posts.Update(post);
        await _uow.SaveChangesAsync(ct);

        return new RemovePostMediaResult(post.Id, cmd.MediaFileId);
    }
}
