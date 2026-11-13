using Vitrio.Application.Common.Idempotency;

namespace Vitrio.Application.Common.Repositories;

public interface IIdempotencyKeySearchableRepository
{
    Task<StoredIdempotentResponse?> FindAsync(Guid userId, Guid key, CancellationToken cancellationToken);
}
