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

        if (!_currentUser.IsSuperAdmin)
        {
            var isMember = await _events.IsMemberAsync(q.EventId, q.UserId, ct);
            if (!isMember) throw new UnauthorizedAccessException("Not allowed.");
        }

        var posts = await _posts.GetByEventIdPagedAsync(q.EventId, q.Page, q.PageSize, ct);

        if (posts.Count == 0)
            return new GetEventPostsResult(q.EventId, q.Page, q.PageSize, new());

        var userIds = posts
            .Where(p => p.CreatedByUserId.HasValue)
            .Select(p => p.CreatedByUserId!.Value)
            .Distinct()
            .ToList();

        var userMap = await _users.GetDisplayNamesByIdsAsync(userIds, ct);

        // Collect ALL media file IDs (cover + all media in posts)
        var allMediaIds = new HashSet<Guid>();
        foreach (var p in posts)
        {
            foreach (var pm in p.Media)
            {
                allMediaIds.Add(pm.MediaFileId);
            }
        }

        var files = allMediaIds.Count == 0
            ? new List<Eventoria.Domain.Entities.MediaFile>()
            : await _mediaFiles.GetByIdsAsync(allMediaIds.ToList(), ct);

        var fileMap = files.ToDictionary(x => x.Id, x => x);

        // IMPORTANT: Collect all thumbnail IDs - these should NOT appear in post media lists
        var thumbnailIds = files
            .Where(f => f.ThumbnailMediaFileId.HasValue)
            .Select(f => f.ThumbnailMediaFileId.Value)
            .ToHashSet();

        // Also load thumbnail files for URL generation
        var thumbnailFiles = thumbnailIds.Count == 0
            ? new List<Eventoria.Domain.Entities.MediaFile>()
            : await _mediaFiles.GetByIdsAsync(thumbnailIds.ToList(), ct);

        var thumbnailMap = thumbnailFiles.ToDictionary(x => x.Id, x => x);

        var ttl = TimeSpan.FromMinutes(15);
        var items = new List<EventPostListItemDto>(posts.Count);

        foreach (var p in posts)
        {
            // Filter media: Exclude thumbnails (they are not part of the post)
            var postMedia = p.Media
                .Where(pm => !thumbnailIds.Contains(pm.MediaFileId))
                .OrderBy(pm => pm.Order)
                .ToList();

            EventPostCoverDto? cover = null;

            // Get cover: first non-thumbnail media
            var coverPm = postMedia.FirstOrDefault();
            if (coverPm != null && fileMap.TryGetValue(coverPm.MediaFileId, out var mf))
            {
                var storage = _resolver.Resolve(mf.ProviderKey);

                var url = await storage.GetDownloadUrlAsync(
                    mf.BucketOrContainer,
                    mf.ObjectKey,
                    validFor: ttl,
                    ct);

                string? thumbnailUrl = null;
                Guid? thumbnailMediaFileId = null;

                // For videos: get thumbnail URL if exists
                if (mf.ThumbnailMediaFileId.HasValue 
                    && thumbnailMap.TryGetValue(mf.ThumbnailMediaFileId.Value, out var thumbnailMf))
                {
                    var thumbnailStorage = _resolver.Resolve(thumbnailMf.ProviderKey);
                    thumbnailUrl = await thumbnailStorage.GetDownloadUrlAsync(
                        thumbnailMf.BucketOrContainer,
                        thumbnailMf.ObjectKey,
                        validFor: ttl,
                        ct);

                    thumbnailMediaFileId = thumbnailMf.Id;
                }

                cover = new EventPostCoverDto(
                    MediaFileId: mf.Id,
                    Order: coverPm.Order,
                    Url: url,
                    ContentType: mf.ContentType,
                    SizeBytes: mf.SizeBytes,
                    ThumbnailUrl: thumbnailUrl,
                    ThumbnailMediaFileId: thumbnailMediaFileId
                );
            }

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
