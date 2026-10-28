namespace Vitrio.Application.Abstractions;

public interface IAppDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
