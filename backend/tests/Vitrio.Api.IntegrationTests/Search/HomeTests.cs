using System.Globalization;
using System.Net;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Providers;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Search.Common;

namespace Vitrio.Api.IntegrationTests.Search;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class HomeTests(ApiFactory factory)
{
    private const string Home = "/api/v1/home";

    private static readonly CitySummary Santos = new(FakeIbgeCitySource.SantosCode, "Santos", "SP");

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Home_ListsOnlyRatedProvidersFromTheHighest()
    {
        var spot = SearchApi.NextSpot();
        var good = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        var best = await PublishAsync(new SearchableProvider(spot.Offset(2)));
        var fewReviews = await PublishAsync(new SearchableProvider(spot.Offset(3)));
        var newcomer = await PublishAsync(new SearchableProvider(spot.Offset(4)));
        await factory.RateAsync(good.Id, 4.5m, 10);
        await factory.RateAsync(best.Id, 4.9m, 30);
        await factory.RateAsync(fewReviews.Id, 5m, 2);

        var home = await HomeAsync(SearchApi.Near(spot));

        Assert.True(home.HasProviders);
        Assert.Equal([best.Id, good.Id], home.TopRated.Select(item => item.Id));
        Assert.DoesNotContain(home.TopRated, item => item.Id == newcomer.Id || item.Id == fewReviews.Id);
        Assert.Equal(15, home.Categories.Count);
        Assert.Equal("Elétrica", home.Categories[0].Name);
    }

    [Fact]
    public async Task Home_InACityWithoutProviders_InvitesWithTheCity()
    {
        var home = await HomeAsync($"cityIbgeCode={FakeIbgeCitySource.SaoJoseDosCamposCode}");

        Assert.False(home.HasProviders);
        Assert.Empty(home.TopRated);
        Assert.Equal(FakeIbgeCitySource.SaoJoseDosCamposCode, home.City!.IbgeCode);
        Assert.Equal("São José dos Campos", home.City.Name);
    }

    [Fact]
    public async Task Home_WithOnlyNewcomers_HasProvidersButNoTopRated()
    {
        var spot = SearchApi.NextSpot();
        await PublishAsync(new SearchableProvider(spot.Offset(1)));

        var home = await HomeAsync(SearchApi.Near(spot));

        Assert.True(home.HasProviders);
        Assert.Empty(home.TopRated);
    }

    [Fact]
    public async Task Home_NeverShowsPausedProviders()
    {
        var spot = SearchApi.NextSpot();
        var paused = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        await factory.RateAsync(paused.Id, 4.8m, 10);
        Assert.True((await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/pause", paused.AccessToken)).IsSuccessStatusCode);

        var home = await HomeAsync(SearchApi.Near(spot));

        Assert.False(home.HasProviders);
        Assert.Empty(home.TopRated);
    }

    [Fact]
    public async Task Home_InTheSameCell_MeasuresTheDistanceFromEachPoint()
    {
        var spot = SearchApi.NextSpot();
        var provider = await PublishAsync(new SearchableProvider(spot.Offset(5)));
        await factory.RateAsync(provider.Id, 4.7m, 8);

        var fromSpot = await HomeAsync(SearchApi.Near(spot));
        var fromCloser = await HomeAsync(SearchApi.Near(spot.Offset(0.6)));

        var far = Assert.Single(fromSpot.TopRated).DistanceKm!.Value;
        var near = Assert.Single(fromCloser.TopRated).DistanceKm!.Value;
        Assert.InRange(far - near, 0.4, 0.8);
    }

    [Fact]
    public async Task Home_AvailableTodayTurnsOffAtLocalMidnightEvenWhileCached()
    {
        var spot = SearchApi.NextSpot();
        var provider = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        await factory.RateAsync(provider.Id, 4.6m, 6);
        await _client.SendJsonAsAsync(HttpMethod.Put, $"{ProviderApi.Provider}/available-today", provider.AccessToken, new { value = true });

        var today = await HomeAsync(SearchApi.Near(spot));
        factory.Clock.Advance(TimeSpan.FromDays(1));
        var tomorrow = await HomeAsync(SearchApi.Near(spot));

        Assert.True(Assert.Single(today.TopRated).Badges.AvailableToday);
        Assert.False(Assert.Single(tomorrow.TopRated).Badges.AvailableToday);
    }

    [Fact]
    public async Task Home_InAChosenCity_ListsRatedProvidersWithoutDistance()
    {
        var provider = await PublishAsync(new SearchableProvider(new GeoSpot(-23.95, -46.33)) { City = Santos });
        await factory.RateAsync(provider.Id, 4.9m, 40);

        var home = await HomeAsync($"cityIbgeCode={Santos.IbgeCode}");

        var item = Assert.Single(home.TopRated, candidate => candidate.Id == provider.Id);
        Assert.Null(item.DistanceKm);
        Assert.Equal(Santos, home.City);
    }

    [Fact]
    public async Task Home_NeverExposesPhoneAddressOrCoordinates()
    {
        var spot = SearchApi.NextSpot();
        var provider = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        await factory.RateAsync(provider.Id, 4.8m, 10);

        var response = await _client.GetAsync($"{Home}?{SearchApi.Near(spot)}", TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Carlos Menezes", json, StringComparison.Ordinal);
        Assert.DoesNotContain("991234567", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Rua de Teste", json, StringComparison.Ordinal);
        Assert.DoesNotContain("atitude", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ongitude", json, StringComparison.Ordinal);
        Assert.DoesNotContain("radius", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(spot.Offset(1).Latitude.ToString("F3", CultureInfo.InvariantCulture), json, StringComparison.Ordinal);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("lat=-21.25")]
    [InlineData("lat=10&lng=-48.32")]
    public async Task Home_WithoutAValidLocation_ReturnsBadRequest(string query)
    {
        var response = await _client.GetAsync($"{Home}?{query}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private Task<PublishedProvider> PublishAsync(SearchableProvider provider) => factory.PublishAsync(_client, provider);

    private async Task<HomeResponse> HomeAsync(string query)
    {
        var response = await _client.GetAsync($"{Home}?{query}", TestContext.Current.CancellationToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{query}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadApiAsync<HomeResponse>();
    }
}
