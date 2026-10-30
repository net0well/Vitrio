using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Accounts.Services;

internal sealed class SessionService(
    ISessionWritableRepository sessions,
    IRefreshTokenWritableRepository refreshTokens,
    IRefreshTokenFactory refreshTokenFactory,
    ITokenIssuer tokenIssuer,
    IAccountRolesService roles,
    IClock clock) : ISessionService
{
    public async Task<AuthResponse> StartAsync(User user, string? deviceName, CancellationToken cancellationToken)
    {
        var userRoles = await roles.ForUserAsync(user.Id, cancellationToken);
        var session = Session.Start(user.Id, deviceName, clock);
        var refreshToken = refreshTokenFactory.Create();
        sessions.Add(session);
        refreshTokens.Add(RefreshToken.IssueFor(session, refreshToken.Hash, clock));

        var accessToken = tokenIssuer.Issue(user, session, userRoles);
        return new AuthResponse(accessToken.Value, accessToken.ExpiresInSeconds, refreshToken.Value, MeResponse.From(user, userRoles));
    }

    public async Task<TokenResponse> RotateAsync(User user, Session session, RefreshToken current, CancellationToken cancellationToken)
    {
        var userRoles = await roles.ForUserAsync(user.Id, cancellationToken);
        var next = refreshTokenFactory.Create();
        var nextToken = RefreshToken.IssueFor(session, next.Hash, clock);
        current.MarkUsed(nextToken.Id, clock);
        refreshTokens.Add(nextToken);
        session.Touch(clock);

        var accessToken = tokenIssuer.Issue(user, session, userRoles);
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
