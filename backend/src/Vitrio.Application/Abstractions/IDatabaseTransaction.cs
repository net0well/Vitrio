namespace Vitrio.Application.Abstractions;

public interface IDatabaseTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
