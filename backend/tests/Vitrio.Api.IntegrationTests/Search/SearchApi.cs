using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Providers;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Search.Common;

namespace Vitrio.Api.IntegrationTests.Search;

public static class SearchApi
{
    public const string Providers = "/api/v1/providers";

    private const double KmPerDegree = 111.32;

    private static int _lastSpot = -1;

    public static GeoSpot NextSpot()
    {
        var index = Interlocked.Increment(ref _lastSpot);
        return new GeoSpot(-6 - (1.5 * (index / 12)), -40 - (1.5 * (index % 12)));
    }

    public static GeoSpot Offset(this GeoSpot center, double northKm, double eastKm = 0) =>
        new(
            center.Latitude + (northKm / KmPerDegree),
            center.Longitude + (eastKm / (KmPerDegree * Math.Cos(center.Latitude * Math.PI / 180))));

    public static string UniqueWord() =>
        string.Concat(Guid.NewGuid().ToString("N").Take(10).Select(hex => char.IsAsciiDigit(hex) ? (char)('a' + (hex - '0')) : (char)('k' + (hex - 'a'))));

    public static async Task<PublishedProvider> PublishAsync(this ApiFactory factory, HttpClient client, SearchableProvider provider)
    {
        var auth = await factory.SignInNewAccountAsync(client);
        await client.SendOkAsync(HttpMethod.Post, ProviderApi.Provider, auth.AccessToken, new { displayName = provider.DisplayName, headline = provider.Headline });
        var accessToken = (await (await client.RefreshAsync(auth.RefreshToken)).ReadTokensAsync()).AccessToken;
        await client.SendOkAsync(HttpMethod.Patch, ProviderApi.Provider, accessToken, new { about = provider.About, contactPhone = "16991234567", whatsAppEnabled = true });
        await client.SendOkAsync(
            HttpMethod.Put,
            $"{ProviderApi.Provider}/categories",
            accessToken,
            new { categoryIds = provider.CategoryIds, primaryCategoryId = provider.CategoryIds[0] });
        var cep = factory.Ceps.Register(provider.City.IbgeCode, provider.City.Name, provider.City.State, provider.Point.Latitude, provider.Point.Longitude);
        await client.SendOkAsync(
            HttpMethod.Put,
            $"{ProviderApi.Provider}/service-area",
            accessToken,
            new { cep, number = "10", radiusKm = provider.RadiusKm, neighborhoods = new[] { "Centro" } });
        await client.SendOkAsync(HttpMethod.Put, $"{ProviderApi.Provider}/services", accessToken, new { services = provider.Services });
        await client.SendOkAsync(HttpMethod.Post, $"{ProviderApi.Provider}/publish", accessToken, new { });
        var created = await (await client.SendAsAsync(HttpMethod.Get, ProviderApi.Provider, accessToken)).Content.ReadApiAsync<ProviderPrivateResponse>();
        return new PublishedProvider(accessToken, created.Id);
    }

    public static Task<HttpResponseMessage> SearchAsync(this HttpClient client, string query) =>
        client.GetAsync($"{Providers}?{query}", TestContext.Current.CancellationToken);

    public static async Task<ProviderSearchPage> SearchPageAsync(this HttpClient client, string query)
    {
        var response = await client.SearchAsync(query);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{query}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadApiAsync<ProviderSearchPage>();
    }

    public static string Near(GeoSpot spot) =>
        string.Create(CultureInfo.InvariantCulture, $"lat={spot.Latitude:F6}&lng={spot.Longitude:F6}");

    public static Task RateAsync(this ApiFactory factory, Guid providerId, decimal score, int count) =>
        factory.QueryAsync(db => db.Database.ExecuteSqlAsync(
            $"update providers set rating_score = {score}, review_count = {count} where id = {providerId}",
            TestContext.Current.CancellationToken));

    private static async Task SendOkAsync(this HttpClient client, HttpMethod method, string url, string accessToken, object body)
    {
        var response = await client.SendJsonAsAsync(method, url, accessToken, body);
        Assert.True(response.IsSuccessStatusCode, $"{method} {url}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }
}
