using Eventoria.Application.Events.Queries.GetEventMediaFeed;
using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IPostRepository : IRepository<Post>
{
    Task<Post?> GetByIdWithMediaAsync(Guid postId, CancellationToken ct);
    Task<bool> IsOwnerAsync(Guid postId, Guid userId, CancellationToken ct);

    Task<List<Post>> GetByEventIdPagedAsync(Guid eventId, int page, int pageSize, CancellationToken ct);

    Task<List<Guid>> GetMediaFileIdsForPostsAsync(IReadOnlyCollection<Guid> postIds, CancellationToken ct);

    Task<GetEventMediaFeedResult> GetEventMediaFeedAsync(Guid eventId, int page, int pageSize, CancellationToken ct);
}
