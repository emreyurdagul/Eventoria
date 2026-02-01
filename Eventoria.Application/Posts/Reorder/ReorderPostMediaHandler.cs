using Eventoria.Application.Abstractions.Persistence;
using MediatR;

namespace Eventoria.Application.Posts.Reorder;

public sealed class ReorderPostMediaHandler : IRequestHandler<ReorderPostMediaCommand, ReorderPostMediaResult>
{
    private readonly IPostRepository _posts;
    private readonly IUnitOfWork _uow;

    public ReorderPostMediaHandler(IPostRepository posts, IUnitOfWork uow)
    {
        _posts = posts;
        _uow = uow;
    }

    public async Task<ReorderPostMediaResult> Handle(ReorderPostMediaCommand cmd, CancellationToken ct)
    {
        if (cmd.OrderedMediaFileIds == null || cmd.OrderedMediaFileIds.Count == 0)
            throw new InvalidOperationException("OrderedMediaFileIds is required.");

        var post = await _posts.GetByIdWithMediaAsync(cmd.PostId, ct)
            ?? throw new InvalidOperationException("Post not found.");

        if (post.CreatedByUserId != cmd.UserId)
            throw new UnauthorizedAccessException("Only post owner can reorder.");

        post.SetMediaOrder(cmd.OrderedMediaFileIds);

        await _uow.SaveChangesAsync(ct);
        return new ReorderPostMediaResult(post.Id);
    }
}
