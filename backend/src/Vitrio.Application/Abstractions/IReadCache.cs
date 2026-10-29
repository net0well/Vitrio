namespace Vitrio.Application.Abstractions;

public interface IReadCache
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan expiration,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken);

    Task RemoveTagAsync(string tag, CancellationToken cancellationToken);
}
