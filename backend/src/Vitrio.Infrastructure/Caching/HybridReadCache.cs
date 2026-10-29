using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Caching;

internal sealed partial class HybridReadCache(HybridCache cache, ILogger<HybridReadCache> logger) : IReadCache
{
    private static readonly TimeSpan MaxLocalLifetime = TimeSpan.FromMinutes(5);

    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan expiration,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            key,
            factory,
            static (factory, token) => factory(token),
            new HybridCacheEntryOptions
            {
                Expiration = expiration,
                LocalCacheExpiration = expiration < MaxLocalLifetime ? expiration : MaxLocalLifetime
            },
            tags,
            cancellationToken);

    public async Task RemoveTagAsync(string tag, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveByTagAsync(tag, cancellationToken);
        }
        catch (RedisException exception)
        {
            LogCacheUnavailable(logger, tag, exception.GetType().Name);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not clear the cache tag {Tag} ({FailureType}); the entries expire on their own")]
    private static partial void LogCacheUnavailable(ILogger logger, string tag, string failureType);
}
