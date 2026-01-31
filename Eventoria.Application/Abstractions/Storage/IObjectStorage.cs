namespace Eventoria.Application.Abstractions.Storage;

public interface IObjectStorage
{
    Task<StoragePutResult> PutAsync(StoragePutRequest request, CancellationToken ct);
    Task DeleteAsync(string bucket, string objectKey, CancellationToken ct);
    Task<string> GetDownloadUrlAsync(string bucket, string objectKey, TimeSpan validFor, CancellationToken ct);
}
