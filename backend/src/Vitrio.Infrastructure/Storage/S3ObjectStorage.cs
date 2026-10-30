using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Util;
using Microsoft.Extensions.Options;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Storage;

internal sealed class S3ObjectStorage(StorageClients clients, IOptions<StorageOptions> options) : IObjectStorage
{
    private const string ImmutablePublicCache = "public, max-age=31536000, immutable";

    private readonly StorageOptions _settings = options.Value;

    public string CreateUploadUrl(string key, string contentType, long contentLength, TimeSpan validFor)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.UploadsBucket,
            Key = key,
            Verb = HttpVerb.PUT,
            // The SDK measures the expiry against its own clock and rejects anything past 7 days, so an absolute time from IClock breaks when the clock is shifted.
            Expires = AWSSDKUtils.CorrectedUtcNow.Add(validFor),
            ContentType = contentType,
            Protocol = (_settings.UploadServiceUrl ?? _settings.ServiceUrl).StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? Protocol.HTTP : Protocol.HTTPS
        };
        request.Headers.ContentLength = contentLength;
        return clients.Uploads.GetPreSignedURL(request);
    }

    public async Task<StoredUpload?> FindUploadAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var metadata = await clients.Server.GetObjectMetadataAsync(_settings.UploadsBucket, key, cancellationToken);
            return new StoredUpload(metadata.ContentLength, metadata.Headers.ContentType);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Stream> OpenUploadAsync(string key, CancellationToken cancellationToken)
    {
        using var response = await clients.Server.GetObjectAsync(_settings.UploadsBucket, key, cancellationToken);
        var copy = new MemoryStream();
        await response.ResponseStream.CopyToAsync(copy, cancellationToken);
        copy.Position = 0;
        return copy;
    }

    public Task DeleteUploadAsync(string key, CancellationToken cancellationToken) =>
        clients.Server.DeleteObjectAsync(_settings.UploadsBucket, key, cancellationToken);

    public async Task<IReadOnlyList<string>> ListUploadsCreatedBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken)
    {
        var keys = new List<string>();
        var request = new ListObjectsV2Request { BucketName = _settings.UploadsBucket };
        ListObjectsV2Response response;
        do
        {
            response = await clients.Server.ListObjectsV2Async(request, cancellationToken);
            keys.AddRange((response.S3Objects ?? [])
                .Where(stored => stored.LastModified is { } modified && modified.ToUniversalTime() < before.UtcDateTime)
                .Select(stored => stored.Key));
            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);

        return keys;
    }

    public async Task SavePublicAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        using var body = new MemoryStream(content);
        var request = new PutObjectRequest
        {
            BucketName = _settings.PublicBucket,
            Key = key,
            InputStream = body,
            ContentType = contentType
        };
        request.Headers.CacheControl = ImmutablePublicCache;
        await clients.Server.PutObjectAsync(request, cancellationToken);
    }

    public Task DeletePublicAsync(string key, CancellationToken cancellationToken) =>
        clients.Server.DeleteObjectAsync(_settings.PublicBucket, key, cancellationToken);
}
