using System;

namespace cosmos_distributed_cache.Options
{
    public class CosmosOptions
    {
        public Uri ServiceEndpoint { get; set; }

        public string AuthKey { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public string ContainerName { get; set; } = string.Empty;
    }
}
