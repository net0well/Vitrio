using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Providers;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Api.IntegrationTests.Search;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class ProviderSearchTests(ApiFactory factory)
{
    private const string PainterAbout = "Pinto casas e apartamentos há dez anos, com acabamento caprichado e limpeza no fim.";

    private static readonly CitySummary SaoPaulo = new(FakeIbgeCitySource.SaoPauloCode, "São Paulo", "SP");
    private static readonly CitySummary Santos = new(FakeIbgeCitySource.SantosCode, "Santos", "SP");
    private static readonly object[] PaintingServices = [new { title = "Pintura de parede", priceFrom = 300m, priceUnit = "perService" }];

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Search_SortedByDistance_ListsTheNearestFirstWithRoundedDistances()
    {
        var spot = SearchApi.NextSpot();
        var far = await PublishAsync(new SearchableProvider(spot.Offset(12)));
        var near = await PublishAsync(new SearchableProvider(spot.Offset(-2)));

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&sort=distance");

        Assert.Equal([near.Id, far.Id], page.Items.Select(item => item.Id));
        var nearDistance = page.Items[0].DistanceKm!.Value;
        Assert.InRange(nearDistance, 1.5, 2.5);
        Assert.Equal(Math.Round(nearDistance, 1), nearDistance);
        Assert.Equal(12, page.Items[1].DistanceKm);
        Assert.Equal(SearchMode.NearMe, page.Context.Mode);
    }

    [Fact]
    public async Task Search_MeasuresTheServiceRadiusFromThePublicPoint()
    {
        var spot = SearchApi.NextSpot();
        var inside = await PublishAsync(new SearchableProvider(spot.Offset(4)) { RadiusKm = 5 });
        var baseNearby = await PublishAsync(new SearchableProvider(spot.Offset(2)) { RadiusKm = 5 });
        var publicPoint = spot.Offset(8);
        await factory.QueryAsync(db => db.Database.ExecuteSqlAsync(
            $"update providers set public_latitude = {publicPoint.Latitude}, public_longitude = {publicPoint.Longitude} where id = {baseNearby.Id}",
            TestContext.Current.CancellationToken));

        var page = await _client.SearchPageAsync(SearchApi.Near(spot));

        Assert.Contains(page.Items, item => item.Id == inside.Id);
        Assert.DoesNotContain(page.Items, item => item.Id == baseNearby.Id);
    }

    [Fact]
    public async Task Search_WithATypo_FindsElectricians()
    {
        var spot = SearchApi.NextSpot();
        var electrician = await PublishAsync(new SearchableProvider(spot.Offset(1)) { Headline = "Eletricista residencial" });
        var painter = await PublishAsync(new SearchableProvider(spot.Offset(2))
        {
            Headline = "Pintor",
            About = PainterAbout,
            CategoryIds = [4],
            Services = PaintingServices
        });

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&q=eletrisista");

        Assert.Contains(page.Items, item => item.Id == electrician.Id);
        Assert.DoesNotContain(page.Items, item => item.Id == painter.Id);
    }

    [Fact]
    public async Task Search_WithoutAccent_FindsPaintingInTheHeadline()
    {
        var spot = SearchApi.NextSpot();
        var painter = await PublishAsync(new SearchableProvider(spot.Offset(1))
        {
            Headline = "Pintura residencial",
            About = PainterAbout,
            CategoryIds = [4],
            Services = PaintingServices
        });
        var electrician = await PublishAsync(new SearchableProvider(spot.Offset(2)));

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&q=pintor");

        Assert.Contains(page.Items, item => item.Id == painter.Id);
        Assert.DoesNotContain(page.Items, item => item.Id == electrician.Id);
    }

    [Fact]
    public async Task Search_ByCityWithoutCoordinates_ListsWithoutDistance()
    {
        var word = SearchApi.UniqueWord();
        var provider = await PublishAsync(new SearchableProvider(new GeoSpot(-23.55, -46.63)) { City = SaoPaulo, Headline = $"Eletricista {word}" });

        var page = await _client.SearchPageAsync($"cityIbgeCode={SaoPaulo.IbgeCode}&q={word}");

        Assert.Equal(SearchMode.City, page.Context.Mode);
        Assert.Equal(SaoPaulo, page.Context.City);
        Assert.Equal([provider.Id], page.Items.Select(item => item.Id));
        Assert.All(page.Items, item => Assert.Null(item.DistanceKm));
    }

    [Fact]
    public async Task Search_SortedByDistanceWithoutCoordinates_ReturnsBadRequest()
    {
        var response = await _client.SearchAsync($"cityIbgeCode={SaoPaulo.IbgeCode}&sort=distance");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_SortedByDistanceWithAPointInAnotherCity_ReturnsBadRequest()
    {
        var response = await _client.SearchAsync($"lat=-21.25&lng=-48.32&cityIbgeCode={SaoPaulo.IbgeCode}&sort=distance");

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "search.needs_location");
    }

    [Fact]
    public async Task Search_WithMinimumRating_ListsOnlyRatedProvidersAtOrAbove()
    {
        var spot = SearchApi.NextSpot();
        var great = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        var good = await PublishAsync(new SearchableProvider(spot.Offset(2)));
        var fewReviews = await PublishAsync(new SearchableProvider(spot.Offset(3)));
        var unrated = await PublishAsync(new SearchableProvider(spot.Offset(4)));
        await factory.RateAsync(great.Id, 4.8m, 10);
        await factory.RateAsync(good.Id, 4.2m, 10);
        await factory.RateAsync(fewReviews.Id, 4.9m, 2);

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&minRating=4.5");

        var item = Assert.Single(page.Items);
        Assert.Equal(great.Id, item.Id);
        Assert.Equal(4.8m, item.Rating!.Score);
        Assert.Equal(10, item.Rating.Count);
        Assert.False(item.IsNew);
        Assert.DoesNotContain(page.Items, candidate => candidate.Id == unrated.Id);
    }

    [Fact]
    public async Task Search_NeverListsPausedProvidersOrSuspendedAccounts()
    {
        var spot = SearchApi.NextSpot();
        var active = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        var paused = await PublishAsync(new SearchableProvider(spot.Offset(2)));
        var suspended = await PublishAsync(new SearchableProvider(spot.Offset(3)));
        Assert.True((await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/pause", paused.AccessToken)).IsSuccessStatusCode);
        await factory.QueryAsync(db => db.Database.ExecuteSqlAsync(
            $"update users set status = 'Suspended' where id = (select user_id from providers where id = {suspended.Id})",
            TestContext.Current.CancellationToken));

        var page = await _client.SearchPageAsync(SearchApi.Near(spot));

        Assert.Equal([active.Id], page.Items.Select(item => item.Id));
    }

    [Theory]
    [InlineData("page=11")]
    [InlineData("pageSize=31")]
    [InlineData("pageSize=0")]
    [InlineData("maxDistanceKm=51")]
    [InlineData("minRating=2")]
    [InlineData("sort=nearest")]
    public async Task Search_OutsideTheLimits_ReturnsBadRequest(string parameter)
    {
        var response = await _client.SearchAsync($"lat=-21.25&lng=-48.32&{parameter}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_OnTheTenthPage_IsAllowed()
    {
        var page = await _client.SearchPageAsync("lat=-21.25&lng=-48.32&page=10");

        Assert.Equal(10, page.Page);
    }

    [Fact]
    public async Task Search_WithoutPointOrCity_ReturnsBadRequest()
    {
        var response = await _client.SearchAsync("q=eletricista");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithAnUnknownCity_ReturnsBadRequest()
    {
        var response = await _client.SearchAsync("cityIbgeCode=1234567");

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "search.city_not_found");
    }

    [Fact]
    public async Task Search_NeverExposesPhoneAddressOrCoordinates()
    {
        var spot = SearchApi.NextSpot();
        await PublishAsync(new SearchableProvider(spot.Offset(1)));

        var response = await _client.SearchAsync(SearchApi.Near(spot));

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Carlos Menezes", json, StringComparison.Ordinal);
        Assert.DoesNotContain("991234567", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Rua de Teste", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Bairro de Teste", json, StringComparison.Ordinal);
        Assert.DoesNotContain("atitude", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ongitude", json, StringComparison.Ordinal);
        Assert.DoesNotContain("cnpj", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Search_ByCategory_CountsTheResultsAndNamesTheProfessional()
    {
        var spot = SearchApi.NextSpot();
        foreach (var km in new[] { 1, 2, 3 })
        {
            await PublishAsync(new SearchableProvider(spot.Offset(km)));
        }

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&categoryId=1");

        Assert.Equal(3, page.Count);
        Assert.False(page.MoreThanCount);
        Assert.Equal(new ProfessionalNameResponse("eletricista", "eletricistas"), page.Context.Professional);
    }

    [Theory]
    [InlineData("Eletricistas", "eletricista")]
    [InlineData("encanador", "encanador")]
    [InlineData("MONTADOR DE MÓVEIS", "montador de móveis")]
    [InlineData("chuveiro", null)]
    public async Task Search_ByATermThatIsAProfession_NamesTheProfessional(string term, string? singular)
    {
        var page = await _client.SearchPageAsync($"{SearchApi.Near(SearchApi.NextSpot())}&q={Uri.EscapeDataString(term)}");

        Assert.Equal(singular, page.Context.Professional?.Singular);
        Assert.Equal(0, page.Count);
    }

    [Fact]
    public async Task Search_WithMoreThanTwoHundredResults_CountsUpToTwoHundred()
    {
        var spot = SearchApi.NextSpot();
        await factory.QueryAsync(db => db.Database.ExecuteSqlAsync(
            $"""
            with owners as (
              insert into users (id, email, display_name, password_hash, password_changed_at, status, accepted_terms_version,
                terms_accepted_at, created_at)
              select gen_random_uuid(), 'contagem-' || gen_random_uuid() || '@exemplo.com.br', 'Vitrine', 'sem-senha', now(), 'Active', '1',
                now(), now()
              from generate_series(1, 201)
              returning id)
            insert into providers (id, user_id, display_name, headline, whats_app_enabled, status, location_approximate,
              service_radius_meters, available_today, handles_emergencies, review_count, completed_orders,
              response_sample_size, late_cancellations90d, created_at, updated_at, public_latitude, public_longitude, city_ibge_code)
            select gen_random_uuid(), id, 'Vitrine', 'Eletricista', false, 'Published', false,
              15000, false, false, 0, 0, 0, 0, now(), now(), {spot.Latitude}, {spot.Longitude}, {FakeIbgeCitySource.SaoJoseCode}
            from owners
            """,
            TestContext.Current.CancellationToken));

        var page = await _client.SearchPageAsync(SearchApi.Near(spot));

        Assert.Equal(200, page.Count);
        Assert.True(page.MoreThanCount);
        Assert.True(page.HasMore);
    }

    [Fact]
    public async Task SearchHomeAndProfile_NeverWriteTheTermOrTheCoordinatesToTheLogs()
    {
        var point = SearchApi.NextSpot().Offset(0.37, 0.53);
        var provider = await PublishAsync(new SearchableProvider(point.Offset(1)));
        var term = SearchApi.UniqueWord();
        var near = SearchApi.Near(point);

        var search = await _client.SearchAsync($"{near}&q={term}");
        var home = await _client.GetAsync($"/api/v1/home?{near}", TestContext.Current.CancellationToken);
        var profile = await _client.GetAsync($"{SearchApi.Providers}/{provider.Id}?{near}", TestContext.Current.CancellationToken);

        Assert.All([search, home, profile], response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        string[] secrets = [term, Digits(point.Latitude), Digits(point.Longitude)];
        Assert.DoesNotContain(factory.Logs.Rendered, line => secrets.Any(secret => line.Contains(secret, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Search_OverSixtyPerMinute_ReturnsTooManyRequestsWithRetryAfter()
    {
        var client = factory.CreateAuthClient();
        for (var attempt = 0; attempt < 60; attempt++)
        {
            Assert.NotEqual(HttpStatusCode.TooManyRequests, (await client.SearchAsync("lat=-21.25&lng=-48.32")).StatusCode);
        }

        var response = await client.SearchAsync("lat=-21.25&lng=-48.32");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.NotNull(response.Headers.RetryAfter);
    }

    [Fact]
    public async Task Search_InABigCityWithoutGps_ListsProvidersBasedThereFarFromTheCenter()
    {
        var word = SearchApi.UniqueWord();
        var southZone = await PublishAsync(new SearchableProvider(new GeoSpot(-23.715, -46.595)) { City = SaoPaulo, RadiusKm = 5, Headline = $"Eletricista {word}" });

        var page = await _client.SearchPageAsync($"cityIbgeCode={SaoPaulo.IbgeCode}&q={word}");

        Assert.Contains(page.Items, item => item.Id == southZone.Id);
    }

    [Fact]
    public async Task Search_InACityWithoutGps_ListsNeighborsWhoseRadiusReachesIt()
    {
        var word = SearchApi.UniqueWord();
        var reaches = await PublishAsync(new SearchableProvider(new GeoSpot(-23.92, -46.34)) { City = Santos, RadiusKm = 15, Headline = $"Eletricista {word}" });
        var shortRadius = await PublishAsync(new SearchableProvider(new GeoSpot(-23.92, -46.34)) { City = Santos, RadiusKm = 5, Headline = $"Eletricista {word}" });

        var page = await _client.SearchPageAsync($"cityIbgeCode={SaoPaulo.IbgeCode}&q={word}");

        Assert.Equal([reaches.Id], page.Items.Select(item => item.Id));
        Assert.DoesNotContain(page.Items, item => item.Id == shortRadius.Id);
    }

    [Fact]
    public async Task Search_ByPrice_ListsServicePricesFirstAndShowsOtherUnits()
    {
        var spot = SearchApi.NextSpot();
        var perSquareMeter = await PublishAsync(new SearchableProvider(spot.Offset(1))
        {
            Services = [new { title = "Pintura de parede", priceFrom = 15m, priceUnit = "perSquareMeter" }]
        });
        var onQuote = await PublishAsync(new SearchableProvider(spot.Offset(2)) { Services = [new { title = "Reforma elétrica", onQuote = true }] });
        var expensive = await PublishAsync(new SearchableProvider(spot.Offset(3))
        {
            Services = [new { title = "Quadro de força", priceFrom = 200m, priceUnit = "perService" }]
        });
        var cheap = await PublishAsync(new SearchableProvider(spot.Offset(4))
        {
            Services =
            [
                new { title = "Visita técnica", priceFrom = 90m, priceUnit = "perHour" },
                new { title = "Troca de tomada", priceFrom = 50m, priceUnit = "perService" }
            ]
        });

        var byPrice = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&sort=price");
        var cappedPrice = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&maxPriceFrom=100");

        Assert.Equal([cheap.Id, expensive.Id], byPrice.Items.Take(2).Select(item => item.Id));
        Assert.Equal(4, byPrice.Items.Count);
        var squareMeter = byPrice.Items.Single(item => item.Id == perSquareMeter.Id);
        Assert.Equal(15m, squareMeter.Price!.From);
        Assert.Equal(PriceUnit.PerSquareMeter, squareMeter.Price.Unit);
        Assert.Null(byPrice.Items.Single(item => item.Id == onQuote.Id).Price);
        Assert.Equal(new ProviderPriceResponse(50m, PriceUnit.PerService), byPrice.Items[0].Price);
        Assert.Equal([cheap.Id], cappedPrice.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Search_WithMaximumDistance_NeverReturnsFartherProviders()
    {
        var spot = SearchApi.NextSpot();
        var five = await PublishAsync(new SearchableProvider(spot.Offset(5)) { RadiusKm = 20 });
        var nine = await PublishAsync(new SearchableProvider(spot.Offset(9)) { RadiusKm = 20 });
        var eleven = await PublishAsync(new SearchableProvider(spot.Offset(11)) { RadiusKm = 20 });

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&maxDistanceKm=10");

        Assert.Equal(new[] { five.Id, nine.Id }.Order(), page.Items.Select(item => item.Id).Order());
        Assert.DoesNotContain(page.Items, item => item.Id == eleven.Id);
        Assert.All(page.Items, item => Assert.True(item.DistanceKm <= 10));
    }

    [Fact]
    public async Task Search_WithAPointInAnotherCity_UsesTheChosenCityWithoutDistance()
    {
        var word = SearchApi.UniqueWord();
        var provider = await PublishAsync(new SearchableProvider(new GeoSpot(-23.55, -46.63)) { City = SaoPaulo, Headline = $"Eletricista {word}" });

        var page = await _client.SearchPageAsync($"lat=-21.25&lng=-48.32&cityIbgeCode={SaoPaulo.IbgeCode}&q={word}");

        Assert.Equal(SearchMode.City, page.Context.Mode);
        Assert.Equal([provider.Id], page.Items.Select(item => item.Id));
        Assert.Null(page.Items[0].DistanceKm);
    }

    [Fact]
    public async Task Search_WithAPointInTheChosenCity_UsesTheDevicePoint()
    {
        var page = await _client.SearchPageAsync($"lat=-21.25&lng=-48.32&cityIbgeCode={FakeIbgeCitySource.JaboticabalCode}");

        Assert.Equal(SearchMode.NearMe, page.Context.Mode);
        Assert.Equal(FakeIbgeCitySource.JaboticabalCode, page.Context.City!.IbgeCode);
    }

    [Fact]
    public async Task Search_TermInAServiceTitle_RanksAboveTheSameTermInAbout()
    {
        var spot = SearchApi.NextSpot();
        var inAbout = await PublishAsync(new SearchableProvider(spot.Offset(1))
        {
            About = "Atendo a região toda e também faço manutenção de chuveiro elétrico com garantia."
        });
        var inTitle = await PublishAsync(new SearchableProvider(spot.Offset(1))
        {
            Services = [new { title = "Instalação de chuveiro", priceFrom = 80m, priceUnit = "perService" }]
        });

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&q=chuveiro");

        Assert.Equal([inTitle.Id, inAbout.Id], page.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Search_ByFullName_RanksTheExactMatchAboveSimilarNamesWithBetterRatings()
    {
        var spot = SearchApi.NextSpot();
        var exact = await PublishAsync(new SearchableProvider(spot.Offset(1)) { DisplayName = "Carlos Pereira Lima" });
        var similar = await PublishAsync(new SearchableProvider(spot.Offset(1)) { DisplayName = "Carla Pereira" });
        await factory.RateAsync(similar.Id, 4.9m, 150);

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&q=Carlos Pereira Lima");

        Assert.Equal(exact.Id, page.Items[0].Id);
    }

    [Fact]
    public async Task Search_SortedByRating_ListsRatedFromHighestThenTheNewOnes()
    {
        var spot = SearchApi.NextSpot();
        var newcomer = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        var good = await PublishAsync(new SearchableProvider(spot.Offset(2)));
        var best = await PublishAsync(new SearchableProvider(spot.Offset(3)));
        var bestWithFewer = await PublishAsync(new SearchableProvider(spot.Offset(4)));
        await factory.RateAsync(good.Id, 4.2m, 10);
        await factory.RateAsync(best.Id, 4.9m, 20);
        await factory.RateAsync(bestWithFewer.Id, 4.9m, 5);

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&sort=rating");

        Assert.Equal([best.Id, bestWithFewer.Id, good.Id, newcomer.Id], page.Items.Select(item => item.Id));
        Assert.True(page.Items[3].IsNew);
        Assert.Null(page.Items[3].Rating);
    }

    [Fact]
    public async Task Search_AfterTheHeadlineChanges_FindsTheNewTerm()
    {
        var spot = SearchApi.NextSpot();
        var word = SearchApi.UniqueWord();
        var provider = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        Assert.Empty((await _client.SearchPageAsync($"{SearchApi.Near(spot)}&q={word}")).Items);

        await _client.SendJsonAsAsync(HttpMethod.Patch, ProviderApi.Provider, provider.AccessToken, new { headline = $"Encanador {word}" });
        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&q={word}");

        Assert.Equal([provider.Id], page.Items.Select(item => item.Id));
        Assert.Equal($"Encanador {word}", page.Items[0].Headline);
    }

    [Fact]
    public async Task Search_ByCategory_ListsOnlyThatCategory()
    {
        var spot = SearchApi.NextSpot();
        await PublishAsync(new SearchableProvider(spot.Offset(1)));
        var painter = await PublishAsync(new SearchableProvider(spot.Offset(2)) { Headline = "Pintor", About = PainterAbout, CategoryIds = [4] });

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&categoryId=4");

        Assert.Equal([painter.Id], page.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Search_FilteringAvailableTodayAndMei_ListsOnlyWhoHasTheBadges()
    {
        var spot = SearchApi.NextSpot();
        var available = await PublishAsync(new SearchableProvider(spot.Offset(1)));
        var verified = await PublishAsync(new SearchableProvider(spot.Offset(2)));
        await PublishAsync(new SearchableProvider(spot.Offset(3)));
        await _client.SendJsonAsAsync(HttpMethod.Put, $"{ProviderApi.Provider}/available-today", available.AccessToken, new { value = true });
        var cnpj = FakeCnpjLookup.NewCnpj();
        factory.Cnpj.AnswerActiveMei(cnpj);
        await _client.SendJsonAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/mei-verification", verified.AccessToken, new { cnpj });

        var today = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&availableToday=true");
        var mei = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&meiVerified=true");

        var availableItem = Assert.Single(today.Items);
        Assert.Equal(available.Id, availableItem.Id);
        Assert.True(availableItem.Badges.AvailableToday);
        var meiItem = Assert.Single(mei.Items);
        Assert.Equal(verified.Id, meiItem.Id);
        Assert.True(meiItem.Badges.MeiVerified);
    }

    [Fact]
    public async Task Search_WhenMoreResultsExist_PagesThroughThem()
    {
        var spot = SearchApi.NextSpot();
        await PublishAsync(new SearchableProvider(spot.Offset(1)));
        await PublishAsync(new SearchableProvider(spot.Offset(2)));
        await PublishAsync(new SearchableProvider(spot.Offset(3)));

        var first = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&pageSize=2");
        var second = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&pageSize=2&page=2");

        Assert.Equal(2, first.Items.Count);
        Assert.True(first.HasMore);
        Assert.Single(second.Items);
        Assert.False(second.HasMore);
        Assert.Empty(first.Items.Select(item => item.Id).Intersect(second.Items.Select(item => item.Id)));
    }

    [Fact]
    public async Task Search_WithOnlyStopwords_SearchesByTheFiltersAlone()
    {
        var spot = SearchApi.NextSpot();
        var provider = await PublishAsync(new SearchableProvider(spot.Offset(1)));

        var page = await _client.SearchPageAsync($"{SearchApi.Near(spot)}&q=de");

        Assert.Equal([provider.Id], page.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Search_ShowsInitialsCityAndNewcomerBadge()
    {
        var spot = SearchApi.NextSpot();
        await PublishAsync(new SearchableProvider(spot.Offset(1)));

        var item = Assert.Single((await _client.SearchPageAsync(SearchApi.Near(spot))).Items);

        Assert.Equal("CM", item.Initials);
        Assert.Equal("São José", item.CityName);
        Assert.True(item.IsNew);
        Assert.Null(item.ResponseTime);
        Assert.Null(item.AvatarUrl);
    }

    private static string Digits(double coordinate) => Math.Abs(coordinate).ToString("F6", CultureInfo.InvariantCulture);

    private Task<PublishedProvider> PublishAsync(SearchableProvider provider) => factory.PublishAsync(_client, provider);
}
