using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Storage;
using MediatR;

namespace Eventoria.Application.Media.Download;

public sealed class GetMediaDownloadUrlHandler : IRequestHandler<GetMediaDownloadUrlQuery, GetMediaDownloadUrlResult>
{
    private readonly IMediaFileRepository _mediaFiles;
    private readonly IEventRepository _events;
    private readonly IStorageProviderResolver _resolver;

    public GetMediaDownloadUrlHandler(
        IMediaFileRepository mediaFiles,
        IEventRepository events,
        IStorageProviderResolver resolver)
    {
        _mediaFiles = mediaFiles;
        _events = events;
        _resolver = resolver;
    }

    public async Task<GetMediaDownloadUrlResult> Handle(GetMediaDownloadUrlQuery query, CancellationToken ct)
    {
        if (query.UserId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (query.MediaFileId == Guid.Empty) throw new InvalidOperationException("MediaFileId is required.");

        var mf = await _mediaFiles.GetByIdAsync(query.MediaFileId, ct)
            ?? throw new InvalidOperationException("Media file not found.");

        // Authorization:
        // - owner always ok
        // - if event context exists, any event member ok
        if (mf.OwnerUserId != query.UserId)
        {
            if (!mf.EventId.HasValue)
                throw new UnauthorizedAccessException("Not allowed.");

            var isMember = await _events.IsMemberAsync(mf.EventId.Value, query.UserId, ct);
            if (!isMember)
                throw new UnauthorizedAccessException("Not allowed.");
        }

        // signed url for everyone (even if visibility public; sen "hepsi signed" dedin)
        var storage = _resolver.Resolve(mf.ProviderKey);
        var url = await storage.GetDownloadUrlAsync(
            mf.BucketOrContainer,
            mf.ObjectKey,
            validFor: TimeSpan.FromMinutes(15),
            ct);

        return new GetMediaDownloadUrlResult(url);
    }
}
