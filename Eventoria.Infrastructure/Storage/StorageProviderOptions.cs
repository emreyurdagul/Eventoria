using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Infrastructure.Storage
{
    public sealed class StorageProviderOptions
    {
        public string Type { get; set; } = "s3-compatible"; // future: azure-blob, local
        public string Endpoint { get; set; } = default!;
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string Region { get; set; } = "auto";
        public string Bucket { get; set; } = default!;

        // hepsi signed dediğin için publicBaseUrl şimdilik gereksiz
    }
}
