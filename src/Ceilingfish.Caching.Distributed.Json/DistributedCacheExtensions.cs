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
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
        {
            return SetAsync<T>(cache, key, value, default);
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, CancellationToken cancellationToken)
        {
            var serialized = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            return cache.SetAsync(key, bytes, cancellationToken);
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var bytes = await cache.GetAsync(key).ConfigureAwait(false);
            var serialized = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(serialized);
        }
    }
}
