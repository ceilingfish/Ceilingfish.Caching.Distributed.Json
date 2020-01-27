using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ceilingfish.Caching.Distributed.Json
{
    public static class DistributedCacheExtensions
    {
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, CancellationToken cancellationToken = default)
        {
            return SetAsync<T>(cache, key, value, new DistributedCacheEntryOptions(), cancellationToken);
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            var serialized = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            return cache.SetAsync(key, bytes, options, cancellationToken);
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var bytes = await cache.GetAsync(key).ConfigureAwait(false);
            if(bytes == null)
            {
                return default;
            }

            var serialized = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(serialized);
        }
    }
}
