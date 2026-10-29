using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Services;

internal sealed class SessionService(
    ISessionWritableRepository sessions,
    IRefreshTokenWritableRepository refreshTokens,
    IRefreshTokenFactory refreshTokenFactory,
    ITokenIssuer tokenIssuer,
    IClock clock) : ISessionService
{
    public AuthResponse Start(User user, string? deviceName)
    {
        var session = Session.Start(user.Id, deviceName, clock);
        var refreshToken = refreshTokenFactory.Create();
        sessions.Add(session);
        refreshTokens.Add(RefreshToken.IssueFor(session, refreshToken.Hash, clock));

        var accessToken = tokenIssuer.Issue(user, session, AccountRoles.Customer);
        return new AuthResponse(accessToken.Value, accessToken.ExpiresInSeconds, refreshToken.Value, MeResponse.From(user, AccountRoles.Customer));
    }

    public TokenResponse Rotate(User user, Session session, RefreshToken current)
    {
        var next = refreshTokenFactory.Create();
        var nextToken = RefreshToken.IssueFor(session, next.Hash, clock);
        current.MarkUsed(nextToken.Id, clock);
        refreshTokens.Add(nextToken);
        session.Touch(clock);

        var accessToken = tokenIssuer.Issue(user, session, AccountRoles.Customer);
        return new TokenResponse(accessToken.Value, accessToken.ExpiresInSeconds, next.Value);
    }

    public async Task<IReadOnlyCollection<Guid>> RevokeAsync(
        IReadOnlyCollection<Session> sessionsToRevoke,
        string reason,
        CancellationToken cancellationToken)
    {
        var ids = sessionsToRevoke.Select(session => session.Id).ToList();
        if (ids.Count == 0)
        {
            return ids;
        }

        foreach (var session in sessionsToRevoke)
        {
            session.Revoke(reason, clock);
        }

        foreach (var refreshToken in await refreshTokens.ListUnrevokedForSessionsAsync(ids, cancellationToken))
        {
            refreshToken.Revoke(clock);
        }

        return ids;
    }
}
