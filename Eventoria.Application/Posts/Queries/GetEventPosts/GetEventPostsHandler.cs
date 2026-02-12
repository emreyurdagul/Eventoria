using Eventoria.Application.Abstractions.Auth;
using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Storage;
using MediatR;

namespace Eventoria.Application.Posts.Queries.GetEventPosts;

public sealed class GetEventPostsHandler : IRequestHandler<GetEventPostsQuery, GetEventPostsResult>
{
    private readonly IEventRepository _events;
    private readonly IPostRepository _posts;
    private readonly IMediaFileRepository _mediaFiles;
    private readonly IStorageProviderResolver _resolver;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _users;

    public GetEventPostsHandler(
        IEventRepository events,
        IPostRepository posts,
        IMediaFileRepository mediaFiles,
        IStorageProviderResolver resolver,
        ICurrentUserService currentUser,
        IUserRepository users)
    {
        _events = events;
        _posts = posts;
        _mediaFiles = mediaFiles;
        _resolver = resolver;
        _currentUser = currentUser;
        _users = users;
    }

    public async Task<GetEventPostsResult> Handle(GetEventPostsQuery q, CancellationToken ct)
    {
        if (q.UserId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (q.EventId == Guid.Empty) throw new InvalidOperationException("EventId is required.");
        if (q.Page <= 0) throw new InvalidOperationException("Page must be >= 1.");
        if (q.PageSize <= 0 || q.PageSize > 100) throw new InvalidOperationException("PageSize must be 1..100.");

        // SuperAdmin kontrolü bypass
        if (!_currentUser.IsSuperAdmin)
        {
            var isMember = await _events.IsMemberAsync(q.EventId, q.UserId, ct);
            if (!isMember) throw new UnauthorizedAccessException("Not allowed.");
        }

        var posts = await _posts.GetByEventIdPagedAsync(q.EventId, q.Page, q.PageSize, ct);

        if (posts.Count == 0)
            return new GetEventPostsResult(q.EventId, q.Page, q.PageSize, new());

        // Kullanýcý ID'lerini topla
        var userIds = posts
            .Where(p => p.CreatedByUserId.HasValue)
            .Select(p => p.CreatedByUserId!.Value)
            .Distinct()
            .ToList();

        // Kullanýcý displayName'lerini çek
        var userMap = await _users.GetDisplayNamesByIdsAsync(userIds, ct);

        // ? Sadece cover media id'leri (her post için order en küçük olan)
        var coverIds = posts
            .Select(p => p.Media.OrderBy(m => m.Order).FirstOrDefault())
            .Where(pm => pm != null)
            .Select(pm => pm!.MediaFileId)
            .Distinct()
            .ToList();

        var files = coverIds.Count == 0
            ? new List<Eventoria.Domain.Entities.MediaFile>()
            : await _mediaFiles.GetByIdsAsync(coverIds, ct);

        var fileMap = files.ToDictionary(x => x.Id, x => x);

        var ttl = TimeSpan.FromMinutes(15);

        var items = new List<EventPostListItemDto>(posts.Count);

        foreach (var p in posts)
        {
            EventPostCoverDto? cover = null;

            var coverPm = p.Media.OrderBy(m => m.Order).FirstOrDefault();
            if (coverPm != null && fileMap.TryGetValue(coverPm.MediaFileId, out var mf))
            {
                var storage = _resolver.Resolve(mf.ProviderKey);

                var url = await storage.GetDownloadUrlAsync(
                    mf.BucketOrContainer,
                    mf.ObjectKey,
                    validFor: ttl,
                    ct);

                cover = new EventPostCoverDto(
                    MediaFileId: mf.Id,
                    Order: coverPm.Order,
                    Url: url,
                    ContentType: mf.ContentType,
                    SizeBytes: mf.SizeBytes
                );
            }

            // Kullanýcý displayName'ini bul
            string? displayName = null;
            if (p.CreatedByUserId.HasValue && userMap.TryGetValue(p.CreatedByUserId.Value, out var userName))
            {
                displayName = userName;
            }

            items.Add(new EventPostListItemDto(
                PostId: p.Id,
                CreatedByUserId: p.CreatedByUserId,
                CreatedByDisplayName: displayName,
                Caption: p.Caption,
                CreatedAtUtc: p.CreatedAtUtc,
                Cover: cover
            ));
        }

        return new GetEventPostsResult(q.EventId, q.Page, q.PageSize, items);
    }
}
