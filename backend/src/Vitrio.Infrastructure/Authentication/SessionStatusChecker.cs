using Microsoft.Extensions.Caching.Hybrid;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Repositories;

namespace Vitrio.Infrastructure.Authentication;

internal sealed class SessionStatusChecker(HybridCache cache, ISessionSearchableRepository sessions) : ISessionStatusChecker
{
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(1),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };

    public async Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var revoked = await cache.GetOrCreateAsync(
            SessionStatusKeys.RevokedFlag(sessionId),
            (sessions, sessionId),
            static async (state, token) => !await state.sessions.IsActiveAsync(state.sessionId, token),
            CacheOptions,
            cancellationToken: cancellationToken);
        return !revoked;
    }
}
