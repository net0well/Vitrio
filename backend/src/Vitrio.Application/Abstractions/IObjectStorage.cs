namespace Vitrio.Application.Abstractions;

public interface IObjectStorage
{
    string CreateUploadUrl(string key, string contentType, long contentLength, TimeSpan validFor);

    Task<StoredUpload?> FindUploadAsync(string key, CancellationToken cancellationToken);

    Task<Stream> OpenUploadAsync(string key, CancellationToken cancellationToken);

    Task DeleteUploadAsync(string key, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListUploadsCreatedBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken);

    Task SavePublicAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken);

    Task DeletePublicAsync(string key, CancellationToken cancellationToken);
}
