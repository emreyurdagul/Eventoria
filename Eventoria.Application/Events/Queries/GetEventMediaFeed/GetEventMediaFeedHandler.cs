using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Storage;
using MediatR;

namespace Eventoria.Application.Events.Queries.GetEventMediaFeed;

public sealed class GetEventMediaFeedHandler : IRequestHandler<GetEventMediaFeedQuery, GetEventMediaFeedResult>
{
    private readonly IEventRepository _events;
    private readonly IPostRepository _posts;
    private readonly IMediaFileRepository _mediaFiles;
    private readonly IStorageProviderResolver _resolver;

    public GetEventMediaFeedHandler(
        IEventRepository events,
        IPostRepository posts,
        IMediaFileRepository mediaFiles,
        IStorageProviderResolver resolver)
    {
        _events = events;
        _posts = posts;
        _mediaFiles = mediaFiles;
        _resolver = resolver;
    }

    public async Task<GetEventMediaFeedResult> Handle(GetEventMediaFeedQuery query, CancellationToken ct)
    {
        if (query.EventId == Guid.Empty)
            throw new InvalidOperationException("EventId is required.");

        if (query.Page < 1)
            throw new InvalidOperationException("Page must be >= 1.");

        if (query.PageSize < 1 || query.PageSize > 100)
            throw new InvalidOperationException("PageSize must be between 1 and 100.");

        // Verify event exists
        var eventExists = await _events.GetByIdAsync(query.EventId, ct);
        if (eventExists == null)
            throw new InvalidOperationException("Event not found.");

        // Check if user is event member (if userId provided)
        if (query.UserId.HasValue && query.UserId != Guid.Empty)
        {
            var isMember = await _events.IsMemberAsync(query.EventId, query.UserId.Value, ct);
            if (!isMember)
                throw new UnauthorizedAccessException("User is not a member of this event.");
        }

        // Get paginated media feed with basic data
        var mediaFeedResult = await _posts.GetEventMediaFeedAsync(query.EventId, query.Page, query.PageSize, ct);

        // Extract media file IDs
        var mediaFileIds = mediaFeedResult.MediaItems
            .Select(item => item.MediaId)
            .Distinct()
            .ToList();

        // Get actual media files from database
        var mediaFiles = mediaFileIds.Count == 0
            ? new List<Eventoria.Domain.Entities.MediaFile>()
            : await _mediaFiles.GetByIdsAsync(mediaFileIds, ct);

        var mediaFileMap = mediaFiles.ToDictionary(x => x.Id);

        var ttl = TimeSpan.FromMinutes(15);

        // Generate URLs for each media item
        var itemsWithUrls = new List<EventMediaFeedItemDto>();

        foreach (var item in mediaFeedResult.MediaItems)
        {
            if (mediaFileMap.TryGetValue(item.MediaId, out var mf))
            {
                var storage = _resolver.Resolve(mf.ProviderKey);

                var downloadUrl = await storage.GetDownloadUrlAsync(
                    mf.BucketOrContainer,
                    mf.ObjectKey,
                    validFor: ttl,
                    ct);

                var itemWithUrl = new EventMediaFeedItemDto(
                    MediaId: item.MediaId,
                    PostId: item.PostId,
                    PostTitle: item.PostTitle,
                    PostAuthorId: item.PostAuthorId,
                    PostAuthorName: item.PostAuthorName,
                    MediaType: item.MediaType,
                    ThumbnailUrl: downloadUrl,
                    DownloadUrl: downloadUrl,
                    PostedAtUtc: item.PostedAtUtc,
                    LikeCount: item.LikeCount,
                    CommentCount: item.CommentCount
                );

                itemsWithUrls.Add(itemWithUrl);
            }
        }

        return new GetEventMediaFeedResult(
            itemsWithUrls,
            mediaFeedResult.Page,
            mediaFeedResult.PageSize,
            mediaFeedResult.Total,
            mediaFeedResult.HasMore
        );
    }
}
