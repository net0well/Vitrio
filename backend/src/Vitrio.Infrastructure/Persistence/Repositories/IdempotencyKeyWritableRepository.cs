using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Application.Common.Repositories;
using Vitrio.Infrastructure.Persistence.Idempotency;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class IdempotencyKeyWritableRepository(AppDbContext db) : IIdempotencyKeyWritableRepository
{
    public void Add(IdempotencyKeyEntry entry) =>
        db.IdempotencyKeys.Add(IdempotencyKeyRecord.Create(entry.UserId, entry.Key, entry.RequestHash, entry.ResponseBody, entry.CreatedAt));

    public Task<int> DeleteCreatedBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken) =>
        db.IdempotencyKeys.Where(record => record.CreatedAt < before).ExecuteDeleteAsync(cancellationToken);
}
