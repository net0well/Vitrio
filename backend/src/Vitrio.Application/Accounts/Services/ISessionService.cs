using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Services;

public interface ISessionService
{
    AuthResponse Start(User user, string? deviceName);

    TokenResponse Rotate(User user, Session session, RefreshToken current);

    Task<IReadOnlyCollection<Guid>> RevokeAsync(
        IReadOnlyCollection<Session> sessionsToRevoke,
        string reason,
        CancellationToken cancellationToken);
}
