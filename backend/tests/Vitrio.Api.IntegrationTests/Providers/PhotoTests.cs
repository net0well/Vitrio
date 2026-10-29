using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.CleanUpUploads;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Shared.Time;

namespace Vitrio.Api.IntegrationTests.Providers;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class PhotoTests(ApiFactory factory)
{
    private const string Provider = ProviderApi.Provider;

    private readonly HttpClient _client = factory.CreateAuthClient();
    private readonly HttpClient _storage = new();

    [Fact]
    public async Task UploadedPhoto_IsProcessedAndShownWithoutAnyMetadata()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var original = TestImages.JpegWithGpsExif();
        Assert.Contains("Exif", Encoding.ASCII.GetString(original), StringComparison.Ordinal);

        var accepted = await UploadAsync(accessToken, original, "image/jpeg", caption: "Quadro de luz novo");

        Assert.Equal(PhotoStatus.Processing, accepted.Status);
        await factory.Photos.RunPendingAsync(factory.Services);
        var photo = Assert.Single(await ListAsync(accessToken));
        Assert.Equal(PhotoStatus.Active, photo.Status);
        Assert.Equal("Quadro de luz novo", photo.Caption);
        var published = await _storage.GetByteArrayAsync(photo.Url, TestContext.Current.CancellationToken);
        Assert.Equal("RIFF", Encoding.ASCII.GetString(published, 0, 4));
        Assert.Equal("WEBP", Encoding.ASCII.GetString(published, 8, 4));
        Assert.DoesNotContain("Exif", Encoding.ASCII.GetString(published), StringComparison.OrdinalIgnoreCase);
        using var full = SKBitmap.Decode(published);
        Assert.Equal(1200, full.Width);
        var thumbnail = await _storage.GetByteArrayAsync(photo.ThumbnailUrl, TestContext.Current.CancellationToken);
        Assert.True(thumbnail.Length <= 60 * 1024);
        using var small = SKBitmap.Decode(thumbnail);
        Assert.Equal(480, small.Width);
    }

    [Fact]
    public async Task PublishedProfile_ShowsTheProcessedPhotosAndThePhotoPages()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        await _client.CompleteAsync(accessToken);
        await _client.SendAsAsync(HttpMethod.Post, $"{Provider}/publish", accessToken);
        await UploadAsync(accessToken, TestImages.Jpeg(), "image/jpeg");
        await UploadAsync(accessToken, TestImages.Png(), "image/png");
        await factory.Photos.RunPendingAsync(factory.Services);
        var id = (await _client.GetProviderAsync(accessToken)).Id;

        var profile = await Eventually.SucceedsAsync(async () =>
            (await (await _client.GetAsync($"/api/v1/providers/{id}", TestContext.Current.CancellationToken)).Content.ReadApiAsync<ProviderPublicResponse>())
                .FirstPhotos.Count == 2);
        var page = await (await _client.GetAsync($"/api/v1/providers/{id}/photos?page=1", TestContext.Current.CancellationToken))
            .Content.ReadApiAsync<PublicPhotosPageResponse>();

        Assert.Equal(2, page.Items.Count);
        Assert.False(page.HasMore);
        Assert.StartsWith(factory.PublicStorageUrl, page.Items[0].Url, StringComparison.Ordinal);
        Assert.True(profile < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task FileThatIsNotAnImage_IsRejectedWithTheReason()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        await UploadAsync(accessToken, Encoding.UTF8.GetBytes("isto não é uma foto"), "image/jpeg");
        await factory.Photos.RunPendingAsync(factory.Services);

        var photo = Assert.Single(await ListAsync(accessToken));
        Assert.Equal(PhotoStatus.Rejected, photo.Status);
        Assert.Null(photo.Url);
        Assert.Equal(PhotoRejections.MessageFor(PhotoRejections.NotAnImage), photo.RejectionReason);
    }

    [Fact]
    public async Task UploadUrl_WithThirtyPhotos_ReturnsConflict()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var providerId = (await _client.GetProviderAsync(accessToken)).Id;
        await SeedPhotosAsync(providerId, Provider30);

        var response = await RequestUploadAsync(accessToken, "image/jpeg", 1000);

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "provider.too_many_photos");
    }

    [Fact]
    public async Task UploadUrl_OnlyAcceptsTheSignedSize()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var upload = await (await RequestUploadAsync(accessToken, "image/jpeg", 1000)).Content.ReadApiAsync<PhotoUploadResponse>();

        var response = await PutAsync(upload.UploadUrl, new byte[5000], "image/jpeg");

        Assert.False(response.IsSuccessStatusCode, $"The storage accepted a body bigger than the signed size ({response.StatusCode}).");
    }

    [Fact]
    public async Task ConfirmPhoto_WithAnotherProvidersUpload_ReturnsNotFound()
    {
        var owner = await factory.SignInAsProviderAsync(_client);
        var upload = await (await RequestUploadAsync(owner, "image/jpeg", TestImages.Jpeg().Length)).Content.ReadApiAsync<PhotoUploadResponse>();
        var intruder = await factory.SignInAsProviderAsync(_client, "Outro Prestador");

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, $"{Provider}/photos", intruder, new { objectKey = upload.ObjectKey });

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "provider.upload_not_found");
    }

    [Fact]
    public async Task DeletePhoto_OfAnotherProvider_ReturnsNotFound()
    {
        var owner = await factory.SignInAsProviderAsync(_client);
        var accepted = await UploadAsync(owner, TestImages.Jpeg(), "image/jpeg");
        var intruder = await factory.SignInAsProviderAsync(_client, "Outro Prestador");

        var response = await _client.SendAsAsync(HttpMethod.Delete, $"{Provider}/photos/{accepted.Id}", intruder);

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "provider.photo_not_found");
        Assert.Single(await ListAsync(owner));
    }

    [Fact]
    public async Task CaptionOrderAndDelete_ChangeTheOwnersList()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var first = await UploadAsync(accessToken, TestImages.Jpeg(), "image/jpeg");
        var second = await UploadAsync(accessToken, TestImages.Png(), "image/png");
        await factory.Photos.RunPendingAsync(factory.Services);

        await _client.SendJsonAsAsync(HttpMethod.Patch, $"{Provider}/photos/{first.Id}", accessToken, new { caption = "Antes" });
        await _client.SendJsonAsAsync(HttpMethod.Put, $"{Provider}/photos/order", accessToken, new { ids = new[] { second.Id, first.Id } });
        var ordered = await ListAsync(accessToken);
        var deleted = await _client.SendAsAsync(HttpMethod.Delete, $"{Provider}/photos/{second.Id}", accessToken);

        Assert.Equal([second.Id, first.Id], ordered.Select(photo => photo.Id));
        Assert.Equal("Antes", ordered[1].Caption);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _storage.GetAsync(ordered[0].Url, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(first.Id, Assert.Single(await ListAsync(accessToken)).Id);
    }

    [Fact]
    public async Task Avatar_IsCroppedToASquareAndReplacedOnTheNextUpload()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        await SetAvatarAsync(accessToken, TestImages.Png(900, 600), "image/png");
        var first = (await _client.GetProviderAsync(accessToken)).AvatarUrl;
        await SetAvatarAsync(accessToken, TestImages.Jpeg(), "image/jpeg");
        var second = (await _client.GetProviderAsync(accessToken)).AvatarUrl;

        Assert.NotNull(first);
        Assert.NotEqual(first, second);
        using var avatar = SKBitmap.Decode(await _storage.GetByteArrayAsync(second, TestContext.Current.CancellationToken));
        Assert.Equal((512, 512), (avatar.Width, avatar.Height));
        Assert.Equal(HttpStatusCode.NotFound, (await _storage.GetAsync(first, TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Cleanup_DeletesUploadsNeverConfirmedAfterADay()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var upload = await (await RequestUploadAsync(accessToken, "image/jpeg", TestImages.Jpeg().Length)).Content.ReadApiAsync<PhotoUploadResponse>();
        await PutAsync(upload.UploadUrl, TestImages.Jpeg(), "image/jpeg");
        factory.Clock.Advance(TimeSpan.FromHours(25));

        await using var scope = factory.Services.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IObjectStorage>();
        Assert.NotNull(await storage.FindUploadAsync(upload.ObjectKey, TestContext.Current.CancellationToken));

        await scope.ServiceProvider.GetRequiredService<ISender>().Send(new CleanUpUploadsCommand(), TestContext.Current.CancellationToken);

        Assert.Null(await storage.FindUploadAsync(upload.ObjectKey, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PhotoRoutes_WithoutTokenOrProfile_AreRefused()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PostAsync($"{Provider}/photos/upload-url", null, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.SendJsonAsAsync(HttpMethod.Post, $"{Provider}/photos/upload-url", auth.AccessToken, new { contentType = "image/jpeg", contentLength = 10 })).StatusCode);
    }

    private const int Provider30 = Domain.Providers.Provider.MaxPhotos;

    private async Task<PhotoAcceptedResponse> UploadAsync(string accessToken, byte[] content, string contentType, string? caption = null)
    {
        var upload = await (await RequestUploadAsync(accessToken, contentType, content.Length)).Content.ReadApiAsync<PhotoUploadResponse>();
        var stored = await PutAsync(upload.UploadUrl, content, contentType);
        Assert.True(stored.IsSuccessStatusCode, $"Upload failed: {stored.StatusCode} {await stored.Content.ReadAsStringAsync()}");
        var confirmed = await _client.SendJsonAsAsync(HttpMethod.Post, $"{Provider}/photos", accessToken, new { objectKey = upload.ObjectKey, caption });
        Assert.Equal(HttpStatusCode.Accepted, confirmed.StatusCode);
        return await confirmed.Content.ReadApiAsync<PhotoAcceptedResponse>();
    }

    private async Task SetAvatarAsync(string accessToken, byte[] content, string contentType)
    {
        var upload = await (await _client.SendJsonAsAsync(
            HttpMethod.Post,
            $"{Provider}/photos/upload-url",
            accessToken,
            new { purpose = "avatar", contentType, contentLength = content.Length })).Content.ReadApiAsync<PhotoUploadResponse>();
        await PutAsync(upload.UploadUrl, content, contentType);
        var accepted = await _client.SendJsonAsAsync(HttpMethod.Put, $"{Provider}/avatar", accessToken, new { objectKey = upload.ObjectKey });
        Assert.Equal(HttpStatusCode.Accepted, accepted.StatusCode);
        await factory.Photos.RunPendingAsync(factory.Services);
    }

    private Task<HttpResponseMessage> RequestUploadAsync(string accessToken, string contentType, long contentLength) =>
        _client.SendJsonAsAsync(HttpMethod.Post, $"{Provider}/photos/upload-url", accessToken, new { purpose = "portfolio", contentType, contentLength });

    private Task<HttpResponseMessage> PutAsync(string url, byte[] content, string contentType)
    {
        var body = new ByteArrayContent(content);
        body.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return _storage.PutAsync(url, body, TestContext.Current.CancellationToken);
    }

    private async Task<IReadOnlyList<ProviderPhotoResponse>> ListAsync(string accessToken) =>
        await (await _client.SendAsAsync(HttpMethod.Get, $"{Provider}/photos", accessToken)).Content.ReadApiAsync<List<ProviderPhotoResponse>>();

    private async Task SeedPhotosAsync(Guid providerId, int count)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var provider = await db.Providers.Include(candidate => candidate.Photos).SingleAsync(candidate => candidate.Id == providerId, TestContext.Current.CancellationToken);
        for (var index = 0; index < count; index++)
        {
            provider.AddPhoto($"{providerId}/seed-{index}", null, clock);
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
