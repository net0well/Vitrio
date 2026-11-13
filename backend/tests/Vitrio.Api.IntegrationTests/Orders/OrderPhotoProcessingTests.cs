using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Orders;
using static Vitrio.Api.IntegrationTests.Orders.OrderApi;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed class OrderPhotoProcessingTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();
    private readonly HttpClient _storage = new();

    [Fact]
    public async Task CreateOrder_WithAPhoto_StoresAPrivateWebpThatOnlyTheOrderLinksTo()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var uploadKey = await UploadAsync(scenario, TestImages.Jpeg(), "image/jpeg");
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow(), [uploadKey]);

        await ProcessPhotosAsync(created.Id);

        var photo = Assert.Single((await _client.GetOrderAsync(scenario.Customer.AccessToken, created.Id)).Photos);
        Assert.Equal(OrderPhotoStatus.Ready, photo.Status);
        var stored = await _storage.GetAsync(photo.Url!, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, stored.StatusCode);
        Assert.Equal("image/webp", stored.Content.Headers.ContentType?.MediaType);
        Assert.Null(await UploadOf(uploadKey));
    }

    [Fact]
    public async Task CreateOrder_WithAFileThatIsNotAnImage_LeavesThePhotoOut()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var uploadKey = await UploadAsync(scenario, "não sou uma foto"u8.ToArray(), "image/jpeg");
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow(), [uploadKey]);

        await ProcessPhotosAsync(created.Id);

        Assert.Empty((await _client.GetOrderAsync(scenario.Customer.AccessToken, created.Id)).Photos);
        Assert.Null(await UploadOf(uploadKey));
    }

    private async Task ProcessPhotosAsync(Guid orderId)
    {
        await factory.UntilAsync(() => Task.FromResult(factory.Photos.HasOrderPhotos(orderId)));
        await factory.Photos.RunOrderPhotosAsync(factory.Services, orderId);
    }

    private async Task<string> UploadAsync(OrderScenario scenario, byte[] content, string contentType)
    {
        var upload = await (await _client.SendJsonAsAsync(
            HttpMethod.Post,
            $"{Route}/photos/upload-url",
            scenario.Customer.AccessToken,
            new { contentType, contentLength = content.Length })).Content.ReadApiAsync<PhotoUploadResponse>();
        var body = new ByteArrayContent(content);
        body.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var stored = await _storage.PutAsync(upload.UploadUrl, body, TestContext.Current.CancellationToken);
        Assert.True(stored.IsSuccessStatusCode, $"{(int)stored.StatusCode}");
        return upload.ObjectKey;
    }

    private async Task<StoredUpload?> UploadOf(string uploadKey)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IObjectStorage>().FindUploadAsync(uploadKey, TestContext.Current.CancellationToken);
    }
}
