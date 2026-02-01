using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IPostRepository : IRepository<Post>
{
    Task<Post?> GetByIdWithMediaAsync(Guid postId, CancellationToken ct);
    Task<bool> IsOwnerAsync(Guid postId, Guid userId, CancellationToken ct);

    // ✅ Feed için
    Task<List<Post>> GetByEventIdPagedAsync(Guid eventId, int page, int pageSize, CancellationToken ct);

    // ✅ Feed için: PostMedia'daki MediaFileId'leri çekip MediaFile'ları toplu döndürme
    Task<List<Guid>> GetMediaFileIdsForPostsAsync(IReadOnlyCollection<Guid> postIds, CancellationToken ct);
}
