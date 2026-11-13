namespace Vitrio.Application.Abstractions;

public interface IPrivateMediaStorage
{
    Task SaveAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken);

    string CreateReadUrl(string key, TimeSpan validFor);

    Task CopyToUploadsAsync(string key, string uploadKey, CancellationToken cancellationToken);

    Task DeleteAsync(string key, CancellationToken cancellationToken);
}
