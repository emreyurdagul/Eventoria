using Amazon.S3;
using Amazon.S3.Model;
using Eventoria.Application.Abstractions.Storage;

namespace Eventoria.Infrastructure.Storage.S3;

public sealed class S3CompatibleObjectStorage : IObjectStorage
{
    private readonly IAmazonS3 _s3;

    public S3CompatibleObjectStorage(IAmazonS3 s3)
    {
        _s3 = s3;
    }

    public async Task<StoragePutResult> PutAsync(StoragePutRequest request, CancellationToken ct)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"eventoria-upload-{Guid.NewGuid():N}");

        try
        {
            await using (var tmp = File.Create(tempPath))
            {
                await request.Content.CopyToAsync(tmp, ct);
            }

            var put = new PutObjectRequest
            {
                BucketName = request.Bucket,
                Key = request.ObjectKey,
                FilePath = tempPath,                 // ✅ InputStream yerine
                ContentType = request.ContentType,

                // ✅ S3-compatible “STREAMING-...-TRAILER” hatasını kesen ayarlar
                UseChunkEncoding = false,
                DisablePayloadSigning = true
            };

            var res = await _s3.PutObjectAsync(put, ct);

            var etag = res.ETag?.Trim('"');
            return new StoragePutResult(ETag: etag, VersionId: res.VersionId);
        }
        finally
        {
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
        }
    }

    public Task DeleteAsync(string bucket, string objectKey, CancellationToken ct)
        => _s3.DeleteObjectAsync(bucket, objectKey, ct);

    public Task<string> GetDownloadUrlAsync(string bucket, string objectKey, TimeSpan validFor, CancellationToken ct)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(validFor)
        };

        var url = _s3.GetPreSignedURL(req);
        return Task.FromResult(url);
    }
}
