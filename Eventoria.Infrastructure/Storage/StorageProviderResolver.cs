using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Eventoria.Application.Abstractions.Storage;
using Eventoria.Infrastructure.Storage.S3;
using Microsoft.Extensions.Options;

namespace Eventoria.Infrastructure.Storage;

public sealed class StorageProviderResolver : IStorageProviderResolver
{
    private readonly StorageOptions _options;

    // cache clients per providerKey
    private readonly Dictionary<string, IObjectStorage> _cache = new(StringComparer.OrdinalIgnoreCase);

    public StorageProviderResolver(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public string DefaultProviderKey => _options.DefaultProviderKey;

    public string GetDefaultBucket(string providerKey)
    {
        var p = GetProvider(providerKey);
        return p.Bucket;
    }

    public IObjectStorage Resolve(string providerKey)
    {
        if (_cache.TryGetValue(providerKey, out var cached))
            return cached;

        var p = GetProvider(providerKey);

        if (!string.Equals(p.Type, "s3-compatible", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Provider type '{p.Type}' not supported yet.");

        var s3 = BuildS3Client(p);
        var storage = new S3CompatibleObjectStorage(s3);

        _cache[providerKey] = storage;
        return storage;
    }

    private StorageProviderOptions GetProvider(string providerKey)
    {
        if (!_options.Providers.TryGetValue(providerKey, out var p))
            throw new InvalidOperationException($"Storage provider '{providerKey}' not configured.");

        return p;
    }

    private static IAmazonS3 BuildS3Client(StorageProviderOptions p)
    {
        var creds = new BasicAWSCredentials(p.AccessKey, p.SecretKey);

        var cfg = new AmazonS3Config
        {
            ServiceURL = p.Endpoint,
            ForcePathStyle = true, // R2/MinIO için genelde gerekli
            AuthenticationRegion = string.IsNullOrWhiteSpace(p.Region) ? "auto" : p.Region
        };

        return new AmazonS3Client(creds, cfg);
    }
}
