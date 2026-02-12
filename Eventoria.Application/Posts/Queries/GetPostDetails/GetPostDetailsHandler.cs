using Eventoria.Application.Abstractions.Auth;
using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Storage;
using MediatR;

namespace Eventoria.Application.Posts.Queries.GetPostDetails;

public sealed class GetPostDetailsHandler : IRequestHandler<GetPostDetailsQuery, GetPostDetailsResult>
{
    private readonly IPostRepository _posts;
    private readonly IEventRepository _events;
    private readonly IMediaFileRepository _mediaFiles;
    private readonly IStorageProviderResolver _resolver;
    private readonly ICurrentUserService _currentUser;

    public GetPostDetailsHandler(
        IPostRepository posts,
        IEventRepository events,
        IMediaFileRepository mediaFiles,
        IStorageProviderResolver resolver,
        ICurrentUserService currentUser)
    {
        _posts = posts;
        _events = events;
        _mediaFiles = mediaFiles;
        _resolver = resolver;
        _currentUser = currentUser;
    }

    public async Task<GetPostDetailsResult> Handle(GetPostDetailsQuery q, CancellationToken ct)
    {
        if (q.UserId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (q.PostId == Guid.Empty) throw new InvalidOperationException("PostId is required.");

        var post = await _posts.GetByIdWithMediaAsync(q.PostId, ct)
            ?? throw new InvalidOperationException("Post not found.");

        // SuperAdmin kontrolü bypass
        if (!_currentUser.IsSuperAdmin)
        {
            var isMember = await _events.IsMemberAsync(post.EventId, q.UserId, ct);
            if (!isMember) throw new UnauthorizedAccessException("Not allowed.");
        }

        var mediaIds = post.Media.Select(m => m.MediaFileId).Distinct().ToList();
        var files = mediaIds.Count == 0
            ? new List<Eventoria.Domain.Entities.MediaFile>()
            : await _mediaFiles.GetByIdsAsync(mediaIds, ct);

        var fileMap = files.ToDictionary(x => x.Id, x => x);

        var ttl = TimeSpan.FromMinutes(15);

        var mediaDtos = new List<PostMediaDto>(post.Media.Count);

        foreach (var pm in post.Media.OrderBy(x => x.Order))
        {
            if (!fileMap.TryGetValue(pm.MediaFileId, out var mf))
                throw new InvalidOperationException("Media file not found for post.");

            var storage = _resolver.Resolve(mf.ProviderKey);

            var url = await storage.GetDownloadUrlAsync(
                mf.BucketOrContainer,
                mf.ObjectKey,
                validFor: ttl,
                ct);

            mediaDtos.Add(new PostMediaDto(
                MediaFileId: mf.Id,
                Order: pm.Order,
                Url: url,
                ContentType: mf.ContentType,
                SizeBytes: mf.SizeBytes
            ));
        }

        return new GetPostDetailsResult(
            PostId: post.Id,
            EventId: post.EventId,
            CreatedByUserId: post.CreatedByUserId,
            Caption: post.Caption,
            CreatedAtUtc: post.CreatedAtUtc,
            Media: mediaDtos
        );
    }
}
