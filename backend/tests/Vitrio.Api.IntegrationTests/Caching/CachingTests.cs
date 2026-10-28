using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Infrastructure;

namespace Vitrio.Api.IntegrationTests.Caching;

public sealed class CachingTests(ApiFactory factory)
{
    [Fact]
    public async Task HybridCache_WithoutRedis_ServesFromMemory()
    {
        var cache = factory.Services.GetRequiredService<HybridCache>();
        var loads = 0;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            await cache.GetOrCreateAsync(
                "tests:memory",
                _ => ValueTask.FromResult(++loads),
                cancellationToken: TestContext.Current.CancellationToken);
        }

        Assert.Equal(1, loads);
    }

    [Fact]
    public async Task HybridCache_WithRedisConfigured_UsesRedisAsSecondLayer()
    {
        await using var services = BuildServices("localhost:6399");

        Assert.IsAssignableFrom<RedisCache>(services.GetRequiredService<IDistributedCache>());
    }

    [Fact]
    public async Task HybridCache_WhenRedisIsUnreachable_StillReturnsTheValueQuickly()
    {
        await using var services = BuildServices("localhost:6399");
        var cache = services.GetRequiredService<HybridCache>();

        var (first, firstElapsed) = await MeasureAsync(cache, "tests:redis-down:1");
        var (second, secondElapsed) = await MeasureAsync(cache, "tests:redis-down:2");

        Assert.Equal("fresh", first);
        Assert.Equal("fresh", second);
        Assert.True(firstElapsed < TimeSpan.FromSeconds(3), $"First call took {firstElapsed}.");
        Assert.True(secondElapsed < TimeSpan.FromMilliseconds(300), $"Second call took {secondElapsed}.");
    }

    private static async Task<(string Value, TimeSpan Elapsed)> MeasureAsync(HybridCache cache, string key)
    {
        var stopwatch = Stopwatch.StartNew();
        var value = await cache.GetOrCreateAsync(
            key,
            _ => ValueTask.FromResult("fresh"),
            cancellationToken: TestContext.Current.CancellationToken);
        return (value, stopwatch.Elapsed);
    }

    private static ServiceProvider BuildServices(string redisConnectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Host=localhost;Database=unused",
                ["ConnectionStrings:Redis"] = redisConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }
}
