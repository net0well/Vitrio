using Vitrio.Application.Accounts.ListSessions;

namespace Vitrio.Application.Accounts.Repositories;

public interface ISessionSearchableRepository
{
    Task<IReadOnlyList<SessionResponse>> ListActiveForUserAsync(
        Guid userId,
        Guid currentSessionId,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken);
}
