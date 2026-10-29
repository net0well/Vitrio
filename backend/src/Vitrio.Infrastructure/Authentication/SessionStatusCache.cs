using Microsoft.Extensions.Caching.Hybrid;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Authentication;

internal sealed class SessionStatusCache(HybridCache cache) : ISessionStatusCache
{
    public async Task ForgetAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken)
    {
        foreach (var sessionId in sessionIds)
        {
            await cache.RemoveAsync(SessionStatusKeys.RevokedFlag(sessionId), cancellationToken);
        }
    }
}
