using Eventoria.Application.Abstractions.Persistence;
using MediatR;

namespace Eventoria.Application.Posts.Update;

public sealed class UpdatePostHandler : IRequestHandler<UpdatePostCommand, UpdatePostResult>
{
    private readonly IPostRepository _posts;
    private readonly IUnitOfWork _uow;

    public UpdatePostHandler(IPostRepository posts, IUnitOfWork uow)
    {
        _posts = posts;
        _uow = uow;
    }

    public async Task<UpdatePostResult> Handle(UpdatePostCommand cmd, CancellationToken ct)
    {
        var post = await _posts.GetByIdAsync(cmd.PostId, ct)
            ?? throw new InvalidOperationException("Post not found.");

        if (post.CreatedByUserId != cmd.UserId)
            throw new UnauthorizedAccessException("Only post owner can update.");

        post.UpdateCaption(cmd.Caption);

        await _uow.SaveChangesAsync(ct);
        return new UpdatePostResult(post.Id);
    }
}
