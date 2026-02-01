using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
using Eventoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eventoria.Infrastructure.Persistence;

public sealed class PostRepository : GenericRepository<Post>, IPostRepository
{
    private readonly AppDbContext _db;

    public PostRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    public Task<Post?> GetByIdWithMediaAsync(Guid postId, CancellationToken ct)
        => _db.Posts
            .Include(x => x.Media)
            .FirstOrDefaultAsync(x => x.Id == postId, ct);

    public Task<bool> IsOwnerAsync(Guid postId, Guid userId, CancellationToken ct)
        => _db.Posts.AnyAsync(x => x.Id == postId && x.CreatedByUserId == userId, ct);

    public Task<List<Post>> GetByEventIdPagedAsync(Guid eventId, int page, int pageSize, CancellationToken ct)
    => _db.Posts
        .AsNoTracking()
        .Include(x => x.Media)
        .Where(x => x.EventId == eventId)
        .OrderByDescending(x => x.CreatedAtUtc)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);

    public Task<List<Guid>> GetMediaFileIdsForPostsAsync(IReadOnlyCollection<Guid> postIds, CancellationToken ct)
        => _db.PostMedias
            .AsNoTracking()
            .Where(pm => postIds.Contains(pm.PostId))
            .Select(pm => pm.MediaFileId)
            .Distinct()
            .ToListAsync(ct);
}
