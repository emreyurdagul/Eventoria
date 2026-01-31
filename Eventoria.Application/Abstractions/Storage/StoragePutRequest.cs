namespace Eventoria.Application.Abstractions.Storage;

public sealed record StoragePutRequest(
    string Bucket,
    string ObjectKey,
    Stream Content,
    string ContentType
);
