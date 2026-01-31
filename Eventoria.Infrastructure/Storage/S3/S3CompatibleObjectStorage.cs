using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
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
        var put = new PutObjectRequest
        {
            BucketName = request.Bucket,
            Key = request.ObjectKey,
            InputStream = request.Content,
            ContentType = request.ContentType,
            AutoCloseStream = false
        };

        var res = await _s3.PutObjectAsync(put, ct);
        return new StoragePutResult(res.ETag);
    }

    public Task DeleteAsync(string bucket, string objectKey, CancellationToken ct)
        => _s3.DeleteObjectAsync(bucket, objectKey, ct);

    public Task<string> GetDownloadUrlAsync(string bucket, string objectKey, TimeSpan validFor, CancellationToken ct)
    {
        // SDK synchronous API ile url üretiyor; ct burada kullanılmıyor
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
