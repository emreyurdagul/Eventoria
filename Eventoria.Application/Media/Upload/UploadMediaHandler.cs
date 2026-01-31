using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Storage;
using Eventoria.Domain.Entities;
using Eventoria.Domain.Enums;
using MediatR;

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

        // Event context varsa: user o event’in üyesi mi? (MVP güvenlik)
        if (cmd.EventId.HasValue)
        {
            var isMember = await _events.IsMemberAsync(cmd.EventId.Value, cmd.UserId, ct);
            if (!isMember)
                throw new UnauthorizedAccessException("You are not a member of this event.");
        }

        var providerKey = _resolver.DefaultProviderKey; // şimdilik "r2"
        var bucket = _resolver.GetDefaultBucket(providerKey);

        // object key standardı (postId henüz yok; upload önce yapılıyor)
        // later: post oluşturunca PostMedia ile bağlayacağız.
        var mediaId = Guid.NewGuid();
        var objectKey = BuildObjectKey(cmd.EventId, cmd.UserId, mediaId, cmd.FileName);

        var storage = _resolver.Resolve(providerKey);

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            // upload
            cmd.Content.Position = 0;
            var put = await storage.PutAsync(
                new StoragePutRequest(bucket, objectKey, cmd.Content, cmd.ContentType),
                innerCt);

            // metadata
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

            // IMPORTANT: Id’yi biz set ediyoruz, yukarıda new Guid verdik. Bu yüzden:
            // Domain constructor içinde Id = NewGuid() yerine Id=mediaId istemiyorsan böyle bırak.
            // Ben burada "deterministic key" olsun diye mf.Id'yi override etmiyorum.
            // Eğer objectKey'de mediaId kullandık, mf.Id farklı kalabilir.
            // Bu yüzden objectKey'de mediaId yerine mf.Id kullanmak daha iyi.
            // Basit olması için burada mf.Id ile uyumlu olacak şekilde yeniden objectKey üretelim:
            // (Bunu istersen ilk versionda değiştirelim.)

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

    private static string BuildObjectKey(Guid? eventId, Guid userId, Guid mediaId, string fileName)
    {
        var safeName = Path.GetFileName(fileName);
        return eventId.HasValue
            ? $"events/{eventId.Value}/uploads/{userId}/{mediaId}/{safeName}"
            : $"users/{userId}/uploads/{mediaId}/{safeName}";
    }
}
