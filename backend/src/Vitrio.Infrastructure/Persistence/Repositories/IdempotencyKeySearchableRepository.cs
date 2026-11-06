using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Application.Common.Repositories;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class IdempotencyKeySearchableRepository(AppDbContext db) : IIdempotencyKeySearchableRepository
{
    public Task<StoredIdempotentResponse?> FindAsync(Guid userId, Guid key, CancellationToken cancellationToken) =>
        db.IdempotencyKeys
            .AsNoTracking()
            .Where(record => record.UserId == userId && record.Key == key)
            .Select(record => new StoredIdempotentResponse(record.RequestHash, record.ResponseBody))
            .SingleOrDefaultAsync(cancellationToken);
}
