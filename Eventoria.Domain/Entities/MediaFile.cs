using Eventoria.Domain.Common;
using Eventoria.Domain.Enums;

namespace Eventoria.Domain.Entities;

public sealed class MediaFile : BaseEntity
{
    private MediaFile() { } // EF

    public MediaFile(
        Guid ownerUserId,
        Guid? eventId,
        MediaVisibility visibility,
        string providerKey,
        string bucketOrContainer,
        string objectKey,
        string originalFileName,
        string contentType,
        long sizeBytes)
    {
        if (ownerUserId == Guid.Empty) throw new ArgumentException("OwnerUserId required");
        if (string.IsNullOrWhiteSpace(providerKey)) throw new ArgumentException("ProviderKey required");
        if (string.IsNullOrWhiteSpace(bucketOrContainer)) throw new ArgumentException("BucketOrContainer required");
        if (string.IsNullOrWhiteSpace(objectKey)) throw new ArgumentException("ObjectKey required");
        if (string.IsNullOrWhiteSpace(originalFileName)) throw new ArgumentException("OriginalFileName required");
        if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("ContentType required");
        if (sizeBytes <= 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));

        Id = Guid.NewGuid();
        OwnerUserId = ownerUserId;
        EventId = eventId;
        Visibility = visibility;

        ProviderKey = providerKey.Trim();
        BucketOrContainer = bucketOrContainer.Trim();
        ObjectKey = objectKey.Trim();

        OriginalFileName = originalFileName.Trim();
        ContentType = contentType.Trim();
        SizeBytes = sizeBytes;

        Status = MediaFileStatus.Ready;
    }

    public Guid OwnerUserId { get; private set; }
    public Guid? EventId { get; private set; }

    public MediaVisibility Visibility { get; private set; }
    public MediaFileStatus Status { get; private set; }

    public string ProviderKey { get; private set; } = default!;
    public string BucketOrContainer { get; private set; } = default!;
    public string ObjectKey { get; private set; } = default!;

    public string OriginalFileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }

    // opsiyonel: ETag, Sha256 vs.
    public string? ETag { get; private set; }
    public string? Sha256 { get; private set; }

    public void MarkFailed()
    {
        Status = MediaFileStatus.Failed;
        SetUpdated();
    }

    public void MarkDeleted()
    {
        Status = MediaFileStatus.Deleted;
        SetUpdated();
    }

    public void SetETag(string? etag)
    {
        ETag = string.IsNullOrWhiteSpace(etag) ? null : etag.Trim();
        SetUpdated();
    }
}
