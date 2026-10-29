using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Authentication;

internal sealed partial class SessionStatusCache(HybridCache cache, ILogger<SessionStatusCache> logger) : ISessionStatusCache
{
    public async Task ForgetAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken)
    {
        foreach (var sessionId in sessionIds)
        {
            try
            {
                await cache.RemoveAsync(SessionStatusKeys.RevokedFlag(sessionId), cancellationToken);
            }
            catch (RedisException exception)
            {
                LogCacheUnavailable(logger, exception.GetType().Name);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not clear the session status cache ({FailureType}); the database still rejects the session and the cache entry expires within a minute")]
    private static partial void LogCacheUnavailable(ILogger logger, string failureType);
}
