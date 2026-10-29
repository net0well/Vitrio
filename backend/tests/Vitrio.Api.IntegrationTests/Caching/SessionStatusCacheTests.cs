using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Vitrio.Infrastructure.Authentication;

namespace Vitrio.Api.IntegrationTests.Caching;

public sealed class SessionStatusCacheTests
{
    [Fact]
    public async Task ForgetAsync_WhenRedisIsDown_DoesNotFailTheRequest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDistributedCache, UnreachableRedis>();
        services.AddHybridCache();
        var cache = new SessionStatusCache(
            services.BuildServiceProvider().GetRequiredService<HybridCache>(),
            NullLogger<SessionStatusCache>.Instance);

        var exception = await Record.ExceptionAsync(() => cache.ForgetAsync([Guid.CreateVersion7()], TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    private sealed class UnreachableRedis : IDistributedCache
    {
        private static RedisConnectionException Down() => new(ConnectionFailureType.UnableToConnect, "Redis is down.");

        public byte[]? Get(string key) => throw Down();

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => throw Down();

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw Down();

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw Down();

        public void Refresh(string key) => throw Down();

        public Task RefreshAsync(string key, CancellationToken token = default) => throw Down();

        public void Remove(string key) => throw Down();

        public Task RemoveAsync(string key, CancellationToken token = default) => throw Down();
    }
}
