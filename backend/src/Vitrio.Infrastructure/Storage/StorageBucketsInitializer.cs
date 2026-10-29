using Amazon.S3.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Vitrio.Infrastructure.Storage;

internal sealed class StorageBucketsInitializer(StorageClients clients, IOptions<StorageOptions> options) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.CreateBuckets)
        {
            return;
        }

        foreach (var bucket in new[] { options.Value.UploadsBucket, options.Value.PublicBucket })
        {
            if (!await AmazonS3Util.DoesS3BucketExistV2Async(clients.Server, bucket))
            {
                await clients.Server.PutBucketAsync(bucket, cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
