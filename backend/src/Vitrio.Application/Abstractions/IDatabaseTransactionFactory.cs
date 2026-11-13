namespace Vitrio.Application.Abstractions;

public interface IDatabaseTransactionFactory
{
    Task<IDatabaseTransaction> BeginAsync(CancellationToken cancellationToken);
}
