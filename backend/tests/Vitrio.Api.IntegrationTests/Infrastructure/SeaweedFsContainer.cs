using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class SeaweedFsContainer : IAsyncDisposable
{
    public const string AccessKey = "vitrio-test";
    public const string SecretKey = "vitrio-test-secret";
    public const string PublicBucket = "vitrio-public";

    private const int S3Port = 8333;

    private readonly IContainer _container = new ContainerBuilder("chrislusf/seaweedfs:4.48")
        .WithEntrypoint("/bin/sh", "-c")
        .WithCommand(
            "printf '{\"identities\":[{\"name\":\"vitrio\",\"credentials\":[{\"accessKey\":\"" + AccessKey + "\",\"secretKey\":\"" + SecretKey
            + "\"}],\"actions\":[\"Admin\",\"Read\",\"List\",\"Tagging\",\"Write\"]},{\"name\":\"anonymous\",\"actions\":[\"Read:" + PublicBucket
            + "\"]}]}' > /tmp/s3.json && exec weed server -dir=/data -ip.bind=0.0.0.0 -master.volumeSizeLimitMB=64 -volume.max=100 -s3 -s3.config=/tmp/s3.json")
        .WithPortBinding(S3Port, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request.ForPort(S3Port).ForStatusCode(HttpStatusCode.Forbidden)))
        .Build();

    public string ServiceUrl => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(S3Port)}";

    public Task StartAsync() => _container.StartAsync();

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
