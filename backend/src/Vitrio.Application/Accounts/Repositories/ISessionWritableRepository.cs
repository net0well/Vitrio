using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface ISessionWritableRepository
{
    Task<Session?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Session?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Session>> ListUnrevokedForUserAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Session session);
}
