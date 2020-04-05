using cosmos_distributed_cache.Options;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Caching.Cosmos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.Caching.Distributed;
using Polly.Registry;
using System;

namespace cosmos_distributed_cache.Extensions
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection AddHttpClientsWithCaching(this IServiceCollection services, CosmosOptions cosmosOptions)
        {
            // Register a typed http client
            services.AddHttpClient<JsonPlaceholderClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
            });

            // Register cosmos cache as a distributed cache
            services.AddCosmosCache((CosmosCacheOptions cacheOptions) =>
            {
                cacheOptions.ContainerName = cosmosOptions.ContainerName;
                cacheOptions.DatabaseName = cosmosOptions.DatabaseName;
                cacheOptions.ClientBuilder = new CosmosClientBuilder(cosmosOptions.ServiceEndpoint.OriginalString, cosmosOptions.AuthKey);
                cacheOptions.CreateIfNotExists = true;
            });

            // Register AsyncCacheProvider for use by Polly
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IDistributedCache>().AsAsyncCacheProvider<string>());

            // Register a Polly cache policy for caching responses received by the JsonPlaceholderClient, using above distributed cache provider.
            services.AddSingleton<IReadOnlyPolicyRegistry<string>, PolicyRegistry>((serviceProvider) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<JsonPlaceholderClient>>();

                PolicyRegistry registry = new PolicyRegistry();

                registry.Add("jsonPlaceHolderClientCachePolicy", Policy.CacheAsync(serviceProvider.GetRequiredService<IAsyncCacheProvider<string>>(), TimeSpan.FromMinutes(1),
                        onCachePut: (context, key) => logger.LogInformation($"Caching '{key}'."),
                        onCacheGet: (context, key) => logger.LogInformation($"Retrieving '{key}' from the cache."),
                        onCachePutError: (context, key, exception) => logger.LogWarning(exception, $"Cannot add '{key}' to the cache."),
                        onCacheGetError: (context, key, exception) => logger.LogWarning(exception, $"Cannot retrieve '{key}' from the cache."),
                        onCacheMiss: (context, key) => logger.LogInformation($"Cache miss for '{key}'.")
                ));

                return registry;
            });

            return services;
        }
    }
}