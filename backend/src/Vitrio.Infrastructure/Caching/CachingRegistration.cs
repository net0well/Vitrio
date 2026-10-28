using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Vitrio.Infrastructure.Caching;

internal static class CachingRegistration
{
    public const string RedisConnectionStringName = "Redis";

    public static IServiceCollection AddVitrioCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5),
            LocalCacheExpiration = TimeSpan.FromSeconds(30)
        });

        var redis = configuration.GetConnectionString(RedisConnectionStringName);
        if (string.IsNullOrWhiteSpace(redis))
        {
            return services;
        }

        var redisOptions = ConfigurationOptions.Parse(redis);
        // Cache is optional: an unreachable Redis must degrade to the local layer instead of failing startup or stalling requests.
        redisOptions.AbortOnConnectFail = false;
        redisOptions.BacklogPolicy = BacklogPolicy.FailFast;
        redisOptions.ConnectTimeout = 1_000;
        redisOptions.AsyncTimeout = 500;
        redisOptions.SyncTimeout = 500;

        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = redisOptions;
            options.InstanceName = "vitrio:";
        });

        return services;
    }
}
