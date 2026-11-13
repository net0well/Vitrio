using Microsoft.EntityFrameworkCore.Storage;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Persistence;

internal sealed class DatabaseTransaction(IDbContextTransaction transaction) : IDatabaseTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
