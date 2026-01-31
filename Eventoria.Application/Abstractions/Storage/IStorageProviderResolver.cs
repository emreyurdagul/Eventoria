namespace Eventoria.Application.Abstractions.Storage;

public interface IStorageProviderResolver
{
    IObjectStorage Resolve(string providerKey);
    string DefaultProviderKey { get; }
    string GetDefaultBucket(string providerKey);
}
