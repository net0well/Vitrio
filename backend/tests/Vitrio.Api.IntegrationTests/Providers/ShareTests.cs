using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.Share;
using Vitrio.Application.Providers.Common;

namespace Vitrio.Api.IntegrationTests.Providers;

public sealed class ShareTests(ApiFactory factory)
{
    private const string BrandSymbolStroke = "M29 30 L50 71 L71 30";

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
    public async Task SharePage_PublishedOrNot_ShowsTheBrandAndTheBrandIconOnTheTab()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client, "Carlos Eletricista");
        await _client.CompleteAsync(accessToken);
        await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/publish", accessToken);
        var published = (await _client.GetProviderAsync(accessToken)).Id;

        foreach (var id in new[] { published, Guid.NewGuid() })
        {
            var response = await _client.GetAsync($"/p/{id}", TestContext.Current.CancellationToken);
            var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            var wordmark = Regex.Match(html, "<div class=\"brand\">(<svg .*?</svg>)</div>", RegexOptions.Singleline);
            Assert.True(wordmark.Success);
            Assert.Contains("aria-label=\"Vitrio\"", wordmark.Value, StringComparison.Ordinal);
            Assert.Contains(BrandSymbolStroke, wordmark.Value, StringComparison.Ordinal);
            var icon = Regex.Match(html, "<link rel=\"icon\" type=\"image/svg\\+xml\" href=\"data:image/svg\\+xml;base64,([A-Za-z0-9+/=]+)\">");
            Assert.True(icon.Success);
            Assert.Contains(BrandSymbolStroke, Encoding.UTF8.GetString(Convert.FromBase64String(icon.Groups[1].Value)), StringComparison.Ordinal);
            var policy = string.Join(';', response.Headers.GetValues("Content-Security-Policy"));
            Assert.Contains("img-src data:;", policy, StringComparison.Ordinal);
            Assert.DoesNotContain("script-src", policy, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void SharePagePolicy_WithAPhoto_AllowsOnlyThePhotoServerAndEmbeddedImages()
    {
        var page = new SharePageResponse(
            true, "https://play.google.com/store/apps/details?id=app.vitrio", "https://vitrio.app/p/1",
            "Carlos", "Eletricista", "Jaboticabal, SP", "Novo no Vitrio", "https://fotos.vitrio.app/p/1/foto.webp?v=2");

        var policy = SharePage.ContentSecurityPolicy(page);

        Assert.Contains("img-src https://fotos.vitrio.app data:;", policy, StringComparison.Ordinal);
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
