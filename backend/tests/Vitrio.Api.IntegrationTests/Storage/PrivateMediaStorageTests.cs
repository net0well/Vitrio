using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Abstractions;

namespace Vitrio.Api.IntegrationTests.Storage;

public sealed class PrivateMediaStorageTests(ApiFactory factory)
{
    private static readonly byte[] Content = [0x52, 0x49, 0x46, 0x46, 0x01, 0x02, 0x03, 0x04];

    private readonly HttpClient _anonymous = new();

    [Fact]
    public async Task SavedFile_IsReadOnlyThroughASignedUrl()
    {
        var key = $"orders/{Guid.NewGuid():N}.webp";
        await using var scope = factory.Services.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IPrivateMediaStorage>();

        await storage.SaveAsync(key, Content, "image/webp", TestContext.Current.CancellationToken);

        var signed = await _anonymous.GetAsync(storage.CreateReadUrl(key, TimeSpan.FromMinutes(10)), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, signed.StatusCode);
        Assert.Equal(Content, await signed.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        var unsigned = await _anonymous.GetAsync(new Uri(storage.CreateReadUrl(key, TimeSpan.FromMinutes(10))).GetLeftPart(UriPartial.Path), TestContext.Current.CancellationToken);
        Assert.NotEqual(HttpStatusCode.OK, unsigned.StatusCode);
    }

    [Fact]
    public async Task CopyToUploads_LeavesAnUploadReadyForANewOrder()
    {
        var key = $"orders/{Guid.NewGuid():N}.webp";
        var uploadKey = $"uploads/{Guid.NewGuid():N}";
        await using var scope = factory.Services.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IPrivateMediaStorage>();
        var uploads = scope.ServiceProvider.GetRequiredService<IObjectStorage>();
        await storage.SaveAsync(key, Content, "image/webp", TestContext.Current.CancellationToken);

        await storage.CopyToUploadsAsync(key, uploadKey, TestContext.Current.CancellationToken);

        var upload = await uploads.FindUploadAsync(uploadKey, TestContext.Current.CancellationToken);
        Assert.Equal(Content.Length, upload!.Size);
        Assert.Equal("image/webp", upload.ContentType);
    }

    [Fact]
    public async Task DeletedFile_IsGone()
    {
        var key = $"orders/{Guid.NewGuid():N}.webp";
        await using var scope = factory.Services.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IPrivateMediaStorage>();
        await storage.SaveAsync(key, Content, "image/webp", TestContext.Current.CancellationToken);

        await storage.DeleteAsync(key, TestContext.Current.CancellationToken);

        var response = await _anonymous.GetAsync(storage.CreateReadUrl(key, TimeSpan.FromMinutes(10)), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
