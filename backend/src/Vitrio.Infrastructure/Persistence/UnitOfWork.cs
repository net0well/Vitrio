using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Abstractions;
using Vitrio.Shared.Exceptions;

namespace Vitrio.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException("The data was changed by another request.", exception);
        }
    }
}
