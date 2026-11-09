namespace Vitrio.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken);
}
