using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.LookupCep;

namespace Vitrio.Api.IntegrationTests.Locations;

public sealed class LocationTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Locate_InsideTheBoundary_ReturnsTheCity()
    {
        var response = await LocateAsync(-21.2550, -48.3220);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var located = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(FakeIbgeCitySource.JaboticabalCode, located.GetProperty("city").GetProperty("ibgeCode").GetInt32());
        Assert.Equal("Jaboticabal", located.GetProperty("city").GetProperty("name").GetString());
        Assert.Equal("SP", located.GetProperty("city").GetProperty("state").GetString());
        Assert.Equal("boundary", located.GetProperty("precision").GetString());
    }

    [Fact]
    public async Task Locate_InTheSeaNearTheCoast_ReturnsTheNearestCity()
    {
        var response = await LocateAsync(-24.0100, -46.3300);

        var located = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(FakeIbgeCitySource.SantosCode, located.GetProperty("city").GetProperty("ibgeCode").GetInt32());
        Assert.Equal("nearest", located.GetProperty("precision").GetString());
    }

    [Fact]
    public async Task Locate_FarFromEveryCity_ReturnsNotFound()
    {
        var response = await LocateAsync(-10.0000, -55.0000);

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "city.not_nearby");
    }

    [Theory]
    [InlineData(40.7128, -74.0060)]
    [InlineData(-21.2550, -20.0000)]
    public async Task Locate_OutsideBrazil_ReturnsValidationErrorPerField(double latitude, double longitude)
    {
        var response = await LocateAsync(latitude, longitude);

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("latitude", out _) || errors.TryGetProperty("longitude", out _));
    }

    [Fact]
    public async Task Locate_NeverWritesTheCoordinatesToTheLogs()
    {
        await LocateAsync(-21.2551, -48.3221);

        Assert.DoesNotContain(factory.Logs.Rendered, line => line.Contains("21.2551", StringComparison.Ordinal) || line.Contains("48.3221", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("sao jose")]
    [InlineData("São José")]
    public async Task SearchCities_IgnoresAccentsAndShowsTheState(string search)
    {
        var response = await _client.GetAsync($"/api/v1/cities?search={Uri.EscapeDataString(search)}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cities = await response.Content.ReadFromJsonAsync<List<CitySummary>>(TestContext.Current.CancellationToken);
        Assert.Contains(new CitySummary(FakeIbgeCitySource.SaoJoseCode, "São José", "SC"), cities!);
        Assert.Contains(new CitySummary(FakeIbgeCitySource.SaoJoseDosCamposCode, "São José dos Campos", "SP"), cities!);
        Assert.Equal("São José", cities![0].Name);
        Assert.True(cities.Count <= 20);
        Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task SearchCities_FiltersByState()
    {
        var cities = await _client.GetFromJsonAsync<List<CitySummary>>("/api/v1/cities?search=sao%20jose&state=SC", TestContext.Current.CancellationToken);

        Assert.All(cities!, city => Assert.Equal("SC", city.State));
    }

    [Theory]
    [InlineData("/api/v1/cities?search=s")]
    [InlineData("/api/v1/cities?search=sao&limit=21")]
    [InlineData("/api/v1/cities?search=sao&state=XX")]
    public async Task SearchCities_WithInvalidQuery_ReturnsValidationError(string url)
    {
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
    }

    [Fact]
    public async Task GetCity_ReturnsTheCentroidAndTimeZone()
    {
        var response = await _client.GetAsync($"/api/v1/cities/{FakeIbgeCitySource.JaboticabalCode}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var city = await response.Content.ReadFromJsonAsync<CityDetails>(TestContext.Current.CancellationToken);
        Assert.Equal("America/Sao_Paulo", city!.TimeZone);
        Assert.InRange(city.Centroid.Lat, -21.32, -21.18);
        Assert.InRange(city.Centroid.Lng, -48.40, -48.25);
        Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task GetCity_WithUnknownCode_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/cities/9999999", TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "city.not_found");
    }

    [Fact]
    public async Task LookupCep_ForSignedInPerson_ReturnsStreetAndCityWithIbgeCode()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendAsAsync(HttpMethod.Get, $"/api/v1/ceps/{FakeCepLookup.Jaboticabal}", auth.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cep = await response.Content.ReadFromJsonAsync<CepResponse>(TestContext.Current.CancellationToken);
        Assert.Equal("Rua Rui Barbosa", cep!.Street);
        Assert.Equal(new CitySummary(FakeIbgeCitySource.JaboticabalCode, "Jaboticabal", "SP"), cep.City);
    }

    [Theory]
    [InlineData(FakeCepLookup.Unknown, HttpStatusCode.NotFound, "cep.not_found")]
    [InlineData(FakeCepLookup.Unreachable, HttpStatusCode.ServiceUnavailable, "address.cep_unavailable")]
    [InlineData("123", HttpStatusCode.BadRequest, "validation")]
    public async Task LookupCep_WithUnknownUnreachableOrInvalidCep_ReturnsTheProblem(string cep, HttpStatusCode status, string code)
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendAsAsync(HttpMethod.Get, $"/api/v1/ceps/{cep}", auth.AccessToken);

        await response.AssertProblemAsync(status, code);
    }

    [Fact]
    public async Task LookupCep_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync($"/api/v1/ceps/{FakeCepLookup.Jaboticabal}", TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "unauthorized");
    }

    private Task<HttpResponseMessage> LocateAsync(double latitude, double longitude) =>
        _client.GetAsync(
            string.Create(CultureInfo.InvariantCulture, $"/api/v1/cities/locate?lat={latitude}&lng={longitude}"),
            TestContext.Current.CancellationToken);
}
