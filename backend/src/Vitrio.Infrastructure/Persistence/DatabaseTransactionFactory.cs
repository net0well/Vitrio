using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Persistence;

internal sealed class DatabaseTransactionFactory(AppDbContext db) : IDatabaseTransactionFactory
{
    public async Task<IDatabaseTransaction> BeginAsync(CancellationToken cancellationToken) =>
        new DatabaseTransaction(await db.Database.BeginTransactionAsync(cancellationToken));
}
