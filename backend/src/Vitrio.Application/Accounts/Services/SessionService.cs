using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Services;

public sealed class SessionService(
    ISessionWritableRepository sessions,
    IRefreshTokenWritableRepository refreshTokens,
    IRefreshTokenFactory refreshTokenFactory,
    ITokenIssuer tokenIssuer,
    IClock clock)
{
    public AuthResponse Start(User user, string? deviceName)
    {
        var session = Session.Start(user.Id, deviceName, clock);
        var refreshToken = refreshTokenFactory.Create();
        sessions.Add(session);
        refreshTokens.Add(RefreshToken.IssueFor(session, refreshToken.Hash, clock));

        IReadOnlyCollection<string> roles = [Roles.Customer];
        var accessToken = tokenIssuer.Issue(user, session, roles);
        return new AuthResponse(accessToken.Value, accessToken.ExpiresInSeconds, refreshToken.Value, MeResponse.From(user, roles));
    }
}
