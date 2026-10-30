using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;

namespace Vitrio.Infrastructure.Storage;

internal sealed class StorageClients : IDisposable
{
    public StorageClients(IOptions<StorageOptions> options)
    {
        var settings = options.Value;
        Server = Create(settings, settings.ServiceUrl);
        Uploads = Create(settings, settings.UploadServiceUrl ?? settings.ServiceUrl);
    }

    public IAmazonS3 Server { get; }

    public IAmazonS3 Uploads { get; }

    public void Dispose()
    {
        Server.Dispose();
        Uploads.Dispose();
    }

    private static AmazonS3Client Create(StorageOptions settings, string serviceUrl) => new(
        new BasicAWSCredentials(settings.AccessKey, settings.SecretKey),
        new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            AuthenticationRegion = settings.Region,
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
        });
}
