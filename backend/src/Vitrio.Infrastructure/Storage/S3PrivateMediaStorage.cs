using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Util;
using Microsoft.Extensions.Options;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Storage;

internal sealed class S3PrivateMediaStorage(StorageClients clients, IOptions<StorageOptions> options) : IPrivateMediaStorage
{
    private readonly StorageOptions _settings = options.Value;

    public async Task SaveAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        using var body = new MemoryStream(content);
        await clients.Server.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = _settings.PrivateBucket,
                Key = key,
                InputStream = body,
                ContentType = contentType
            },
            cancellationToken);
    }

    public string CreateReadUrl(string key, TimeSpan validFor) =>
        clients.Uploads.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _settings.PrivateBucket,
            Key = key,
            Verb = HttpVerb.GET,
            // The SDK measures the expiry against its own clock, so an absolute time from IClock breaks when the clock is shifted.
            Expires = AWSSDKUtils.CorrectedUtcNow.Add(validFor),
            Protocol = (_settings.UploadServiceUrl ?? _settings.ServiceUrl).StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? Protocol.HTTP : Protocol.HTTPS
        });

    public Task CopyToUploadsAsync(string key, string uploadKey, CancellationToken cancellationToken) =>
        clients.Server.CopyObjectAsync(
            new CopyObjectRequest
            {
                SourceBucket = _settings.PrivateBucket,
                SourceKey = key,
                DestinationBucket = _settings.UploadsBucket,
                DestinationKey = uploadKey
            },
            cancellationToken);

    public Task DeleteAsync(string key, CancellationToken cancellationToken) =>
        clients.Server.DeleteObjectAsync(_settings.PrivateBucket, key, cancellationToken);
}
