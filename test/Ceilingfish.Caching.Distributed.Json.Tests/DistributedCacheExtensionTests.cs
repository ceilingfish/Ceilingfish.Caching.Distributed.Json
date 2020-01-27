using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ceilingfish.Caching.Distributed.Json.Tests
{
    public class DistributedCacheExtensionTests
    {
        class Example
        {
            public string Name { get; set; }
        }

        [Fact]
        public async Task SetAsyncSerializesObjectAndStoresBytes()
        {
            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var example = new Example
            {
                Name = "The Name"
            };

            await cache.SetAsync("test", example);

            var bytes = await cache.GetAsync("test");
            var json = Encoding.UTF8.GetString(bytes);

            Assert.Equal(@"{""Name"":""The Name""}", json);
        }

        [Fact]
        public async Task SetAsyncUsesExpiryPolicy()
        {
            var fixedTime = new DateTimeOffset(2020, 1, 27, 21, 20, 0, TimeSpan.Zero);
            var mockTime = Substitute.For<ISystemClock>();
            mockTime.UtcNow.Returns(fixedTime);
            var options = new MemoryDistributedCacheOptions
            {
                Clock = mockTime
            };
            var cache = new MemoryDistributedCache(Options.Create(options));
            var expirationOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = fixedTime.Subtract(TimeSpan.FromSeconds(1))
            };
            await cache.SetAsync("should_be_gone", new Example { Name = "to expire" }, expirationOptions);
            var missing = await cache.GetAsync<Example>("should_be_gone");
            Assert.Null(missing);
        }

        [Fact]
        public async Task GetAsyncDeserializesBytesIntoUtf8()
        {
            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            cache.SetString("test", @"{""Name"":""Geoffrey""}");
            var subject = await cache.GetAsync<Example>("test");
            Assert.IsType<Example>(subject);
            Assert.Equal("Geoffrey", subject.Name);
        }
    }
}
