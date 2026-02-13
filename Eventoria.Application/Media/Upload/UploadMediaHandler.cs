using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Storage;
using Eventoria.Domain.Entities;
using Eventoria.Domain.Enums;
using MediatR;
using System.Text;

namespace Eventoria.Application.Media.Upload;

public sealed class UploadMediaHandler : IRequestHandler<UploadMediaCommand, UploadMediaResult>
{
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private static readonly HashSet<string> AllowedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4"
    };

    private const long MaxImageBytes = 20L * 1024 * 1024;   // 20MB
    private const long MaxVideoBytes = 200L * 1024 * 1024;  // 200MB

    private readonly IMediaFileRepository _mediaFiles;
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly IStorageProviderResolver _resolver;

    public UploadMediaHandler(
        IMediaFileRepository mediaFiles,
        IEventRepository events,
        IUnitOfWork uow,
        IStorageProviderResolver resolver)
    {
        _mediaFiles = mediaFiles;
        _events = events;
        _uow = uow;
        _resolver = resolver;
    }

    public async Task<UploadMediaResult> Handle(UploadMediaCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (cmd.Content == null) throw new InvalidOperationException("Content is required.");
        if (string.IsNullOrWhiteSpace(cmd.FileName)) throw new InvalidOperationException("FileName is required.");
        if (string.IsNullOrWhiteSpace(cmd.ContentType)) throw new InvalidOperationException("ContentType is required.");
        if (cmd.SizeBytes <= 0) throw new InvalidOperationException("SizeBytes must be > 0.");

        ValidateLimits(cmd.ContentType, cmd.SizeBytes);

        // Event context varsa: user o event'in üyesi mi?
        if (cmd.EventId.HasValue)
        {
            var isMember = await _events.IsMemberAsync(cmd.EventId.Value, cmd.UserId, ct);
            if (!isMember)
                throw new UnauthorizedAccessException("You are not a member of this event.");
        }

        var providerKey = _resolver.DefaultProviderKey; // "r2"
        var bucket = _resolver.GetDefaultBucket(providerKey);
        var storage = _resolver.Resolve(providerKey);

        var isVideo = AllowedVideoTypes.Contains(cmd.ContentType);

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            // Eðer video upload ediyorsak ve client thumbnail id gönderiyorsa önce doðrula
            MediaFile? thumbMf = null;
            if (isVideo && cmd.ThumbnailMediaFileId.HasValue)
            {
                thumbMf = await GetAndValidateThumbnailAsync(
                    thumbnailId: cmd.ThumbnailMediaFileId.Value,
                    userId: cmd.UserId,
                    eventId: cmd.EventId,
                    visibility: cmd.Visibility,
                    ct: innerCt);
            }

            // Thumbnail zorunlu olsun istiyorsan bunu aç:
            // if (isVideo && !cmd.ThumbnailMediaFileId.HasValue)
            //     throw new InvalidOperationException("ThumbnailMediaFileId is required for videos.");

            // 1) Upload main file (image or video)
            var mediaId = Guid.NewGuid();
            var objectKey = BuildObjectKey(cmd.EventId, cmd.UserId, mediaId, cmd.FileName);

            cmd.Content.Position = 0;
            var put = await storage.PutAsync(
                new StoragePutRequest(bucket, objectKey, cmd.Content, cmd.ContentType),
                innerCt);

            // 2) Create MediaFile entity
            var mf = new MediaFile(
                ownerUserId: cmd.UserId,
                eventId: cmd.EventId,
                visibility: cmd.Visibility,
                providerKey: providerKey,
                bucketOrContainer: bucket,
                objectKey: objectKey,
                originalFileName: cmd.FileName,
                contentType: cmd.ContentType,
                sizeBytes: cmd.SizeBytes);

            mf.SetETag(put.ETag);

            // 3) If video and thumbnail provided -> link it
            if (isVideo && thumbMf is not null)
            {
                mf.SetThumbnailMediaFileId(thumbMf.Id);
            }

            await _mediaFiles.AddAsync(mf, innerCt);
            await _uow.SaveChangesAsync(innerCt);

            return new UploadMediaResult(mf.Id);
        }, ct);
    }

    private static void ValidateLimits(string contentType, long sizeBytes)
    {
        var isImage = AllowedImageTypes.Contains(contentType);
        var isVideo = AllowedVideoTypes.Contains(contentType);

        if (!isImage && !isVideo)
            throw new InvalidOperationException("Unsupported content type.");

        if (isImage && sizeBytes > MaxImageBytes)
            throw new InvalidOperationException("Image exceeds max size (20MB).");

        if (isVideo && sizeBytes > MaxVideoBytes)
            throw new InvalidOperationException("Video exceeds max size (200MB).");
    }

    private async Task<MediaFile> GetAndValidateThumbnailAsync(
        Guid thumbnailId,
        Guid userId,
        Guid? eventId,
        MediaVisibility visibility,
        CancellationToken ct)
    {
        // Repo'nda GetByIdAsync varsa bunu kullan:
        var thumb = await _mediaFiles.GetByIdAsync(thumbnailId, ct);
        if (thumb is null)
            throw new InvalidOperationException("Thumbnail not found.");

        // Owner doðrulama
        if (thumb.OwnerUserId != userId)
            throw new UnauthorizedAccessException("Thumbnail does not belong to this user.");

        // Scope (event) doðrulama: ayný event scope'unda olmalý
        if (thumb.EventId != eventId)
            throw new InvalidOperationException("Thumbnail and video must belong to the same event scope.");

        // Type doðrulama
        if (!AllowedImageTypes.Contains(thumb.ContentType))
            throw new InvalidOperationException("Thumbnail must be an image (jpeg/png/webp).");

        // Visibility doðrulama (isteðe baðlý ama önerilir)
        if (thumb.Visibility != visibility)
            throw new InvalidOperationException("Thumbnail visibility must match video visibility.");

        return thumb;
    }

    private static string BuildObjectKey(Guid? eventId, Guid userId, Guid mediaId, string fileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        var slug = Slugify(baseName);
        var shortId = mediaId.ToString("N")[..8];

        var safeName = $"{slug}-{shortId}{ext}".ToLowerInvariant();

        return eventId.HasValue
            ? $"events/{eventId.Value}/uploads/{userId}/{safeName}"
            : $"users/{userId}/uploads/{safeName}";
    }

    private static string Slugify(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s.Trim())
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '-');

        return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
    }
}
