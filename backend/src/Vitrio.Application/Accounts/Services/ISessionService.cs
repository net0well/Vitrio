using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Services;

public interface ISessionService
{
    Task<AuthResponse> StartAsync(User user, string? deviceName, CancellationToken cancellationToken);

    Task<TokenResponse> RotateAsync(User user, Session session, RefreshToken current, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> RevokeAsync(
        IReadOnlyCollection<Session> sessionsToRevoke,
        string reason,
        CancellationToken cancellationToken);
}
