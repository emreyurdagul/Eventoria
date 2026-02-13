using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Events.Queries.GetEventMediaFeed;
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

    public async Task<GetEventMediaFeedResult> GetEventMediaFeedAsync(Guid eventId, int page, int pageSize, CancellationToken ct)
    {
        // Get paginated media items with thumbnail info
        var mediaItems = await _db.PostMedias
            .AsNoTracking()
            .Join(
                _db.Posts,
                pm => pm.PostId,
                p => p.Id,
                (pm, p) => new { pm, p })
            .Join(
                _db.MediaFiles,
                x => x.pm.MediaFileId,
                mf => mf.Id,
                (x, mf) => new { x.pm, x.p, mf })
            .Join(
                _db.Users,
                item => item.p.CreatedByUserId,
                u => u.Id,
                (item, u) => new { item.pm, item.p, item.mf, u })
            .Where(item => item.p.EventId == eventId 
                && item.mf.Status.ToString() == "Ready")
            .OrderByDescending(item => item.p.CreatedAtUtc)
            .ThenBy(item => item.pm.Order)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new EventMediaFeedItemDto(
                MediaId: item.mf.Id,
                PostId: item.p.Id,
                PostTitle: item.p.Caption,
                PostAuthorId: item.p.CreatedByUserId.Value,
                PostAuthorName: item.u.DisplayName ?? item.u.Email ?? "Unknown",
                MediaType: item.mf.ContentType.StartsWith("image/") ? "Photo" : "Video",
                ThumbnailMediaFileId: item.mf.ThumbnailMediaFileId, // Include thumbnail ID for videos
                ThumbnailUrl: null,
                DownloadUrl: null,
                PostedAtUtc: item.p.CreatedAtUtc,
                LikeCount: 0,
                CommentCount: 0
            ))
            .ToListAsync(ct);

        // Get total count
        var total = await _db.PostMedias
            .AsNoTracking()
            .Join(
                _db.Posts,
                pm => pm.PostId,
                p => p.Id,
                (pm, p) => new { pm, p })
            .Join(
                _db.MediaFiles,
                x => x.pm.MediaFileId,
                mf => mf.Id,
                (x, mf) => new { x.pm, x.p, mf })
            .Where(item => item.p.EventId == eventId 
                && item.mf.Status.ToString() == "Ready")
            .CountAsync(ct);

        var hasMore = (page * pageSize) < total;

        return new GetEventMediaFeedResult(
            mediaItems,
            page,
            pageSize,
            total,
            hasMore
        );
    }
}
