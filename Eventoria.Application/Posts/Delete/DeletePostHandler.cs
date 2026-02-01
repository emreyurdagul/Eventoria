using Eventoria.Application.Abstractions.Persistence;
using MediatR;

namespace Eventoria.Application.Posts.Delete;

public sealed class DeletePostHandler : IRequestHandler<DeletePostCommand, DeletePostResult>
{
    private readonly IPostRepository _posts;
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;

    public DeletePostHandler(IPostRepository posts, IEventRepository events, IUnitOfWork uow)
    {
        _posts = posts;
        _events = events;
        _uow = uow;
    }

    public async Task<DeletePostResult> Handle(DeletePostCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (cmd.PostId == Guid.Empty) throw new InvalidOperationException("PostId is required.");

        var post = await _posts.GetByIdAsync(cmd.PostId, ct)
            ?? throw new InvalidOperationException("Post not found.");

        // ✅ Yetki: Post owner veya event admin
        var isOwner = post.CreatedByUserId == cmd.UserId;
        var isAdmin = await _events.IsEventAdminAsync(post.EventId, cmd.UserId, ct);

        if (!isOwner && !isAdmin)
            throw new UnauthorizedAccessException("Not allowed.");

        _posts.Remove(post);
        await _uow.SaveChangesAsync(ct);

        return new DeletePostResult(post.Id);
    }
}
