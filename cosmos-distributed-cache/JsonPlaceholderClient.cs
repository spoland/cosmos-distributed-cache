using cosmos_distributed_cache.Models;
using Polly;
using Polly.Caching;
using Polly.Registry;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace cosmos_distributed_cache
{
    public class JsonPlaceholderClient
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncCachePolicy<string> _cachePolicy;

        public JsonPlaceholderClient(HttpClient httpClient, IReadOnlyPolicyRegistry<string> policyRegistry)
        {
            _httpClient = httpClient;

            // Resolve the caching policy to be used with this client
            _cachePolicy = policyRegistry.Get<AsyncCachePolicy<string>>("jsonPlaceHolderClientCachePolicy");
        }

        public async Task<IEnumerable<Post>> Get()
        {
            var policyExecutionContext = new Context($"get-all");

            var response = await _cachePolicy.ExecuteAsync(context => _httpClient.GetStringAsync("/posts"), policyExecutionContext);

            return JsonSerializer.Deserialize<IEnumerable<Post>>(response);
        }

        public async Task<Post> Get(int id)
        {
            var policyExecutionContext = new Context($"get-{id}");

            var response = await _cachePolicy.ExecuteAsync(context => _httpClient.GetStringAsync($"/posts/{id}"), policyExecutionContext);

            return JsonSerializer.Deserialize<Post>(response);
        }
    }
}
