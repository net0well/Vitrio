using System.Net;
using System.Text.Json;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;

namespace Vitrio.Api.IntegrationTests.Providers;

public sealed class ShareTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task SharePage_OfAPublishedShowcase_ShowsTheCardWithOpenGraph()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client, "Carlos Eletricista");
        await _client.CompleteAsync(accessToken);
        await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/publish", accessToken);
        var id = (await _client.GetProviderAsync(accessToken)).Id;

        var response = await _client.GetAsync($"/p/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("<meta property=\"og:title\" content=\"Carlos Eletricista · Eletricista\">", html, StringComparison.Ordinal);
        Assert.Contains($"<meta property=\"og:url\" content=\"https://vitrio.app/p/{id}\">", html, StringComparison.Ordinal);
        Assert.Contains("Jaboticabal, SP", html, StringComparison.Ordinal);
        Assert.Contains("Novo no Vitrio", html, StringComparison.Ordinal);
        Assert.Contains("https://play.google.com/store/apps/details?id=app.vitrio", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Rui Barbosa", html, StringComparison.Ordinal);
        Assert.Equal("public, max-age=300", response.Headers.CacheControl!.ToString());
        Assert.Contains("style-src 'unsafe-inline'", string.Join(';', response.Headers.GetValues("Content-Security-Policy")), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SharePage_EncodesWhatTheProviderTyped()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client, "Zé <b>Pintor</b>");
        await _client.CompleteAsync(accessToken);
        await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/publish", accessToken);
        var id = (await _client.GetProviderAsync(accessToken)).Id;

        var html = await _client.GetStringAsync($"/p/{id}", TestContext.Current.CancellationToken);

        Assert.DoesNotContain("<b>Pintor", html, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;Pintor", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SharePage_OfADraftOrUnknownShowcase_SaysItIsNotAvailable()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var draft = (await _client.GetProviderAsync(accessToken)).Id;

        foreach (var id in new[] { draft, Guid.NewGuid() })
        {
            var response = await _client.GetAsync($"/p/{id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("Perfil não disponível", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task AssetLinks_DeclareThePackageAndTheFingerprintsFromConfiguration()
    {
        var response = await _client.GetAsync("/.well-known/assetlinks.json", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var statement = Assert.Single(json.RootElement.EnumerateArray().ToList());
        Assert.Equal("delegate_permission/common.handle_all_urls", statement.GetProperty("relation")[0].GetString());
        var target = statement.GetProperty("target");
        Assert.Equal("android_app", target.GetProperty("namespace").GetString());
        Assert.Equal("app.vitrio", target.GetProperty("package_name").GetString());
        Assert.Equal(ApiFactory.AndroidFingerprint, target.GetProperty("sha256_cert_fingerprints")[0].GetString());
    }
}
