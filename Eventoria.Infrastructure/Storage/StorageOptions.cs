namespace Eventoria.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string DefaultProviderKey { get; set; } = "r2";
    public Dictionary<string, StorageProviderOptions> Providers { get; set; } = new();
}