using Vitrio.Application.Common.Idempotency;

namespace Vitrio.Application.Common.Repositories;

public interface IIdempotencyKeyWritableRepository
{
    void Add(IdempotencyKeyEntry entry);

    Task<int> DeleteCreatedBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken);
}
