using Microsoft.Extensions.Logging.Abstractions;
using Vitrio.Infrastructure.External.Ibge;

namespace Vitrio.Api.IntegrationTests.External;

public sealed class IbgeCitySourceTests
{
    private const string Localities = """
        [{"municipio-id":3524303,"municipio-nome":"Jaboticabal","UF-sigla":"SP"},
         {"municipio-id":3550308,"municipio-nome":"São Paulo","UF-sigla":"SP"}]
        """;

    private const string Boundaries = """
        {"type":"FeatureCollection","features":[
          {"type":"Feature","geometry":{"type":"Polygon","coordinates":[[[-48.4,-21.3],[-48.2,-21.3],[-48.2,-21.1],[-48.4,-21.3]]]},"properties":{"codarea":"3524303"}},
          {"type":"Feature","geometry":{"type":"MultiPolygon","coordinates":[[[[-46.8,-23.8],[-46.3,-23.8],[-46.3,-23.3],[-46.8,-23.8]]]]},"properties":{"codarea":"3550308"}}]}
        """;

    private const string Populations = """
        [{"id":"9324","resultados":[{"classificacoes":[],"series":[
          {"localidade":{"id":"3524303","nome":"Jaboticabal - SP"},"serie":{"2026":"77251"}},
          {"localidade":{"id":"3550308","nome":"São Paulo - SP"},"serie":{"2026":"11911337"}}]}]}]
        """;

    [Fact]
    public async Task ListStateAsync_JoinsNamesBoundariesAndPopulationByIbgeCode()
    {
        var source = Source(new Dictionary<string, string>
        {
            ["v1/localidades/estados/SP"] = Localities,
            ["v3/malhas/estados/SP"] = Boundaries,
            ["v3/agregados/6579"] = Populations
        });

        var cities = await source.ListStateAsync("SP", TestContext.Current.CancellationToken);

        Assert.Equal([3524303, 3550308], cities.Select(city => city.IbgeCode));
        Assert.Equal([77_251, 11_911_337], cities.Select(city => city.Population));
        Assert.Equal("São Paulo", cities[1].Name);
        Assert.Contains("MultiPolygon", cities[1].BoundaryGeoJson, StringComparison.Ordinal);
        Assert.All(cities, city => Assert.Equal("SP", city.State));
    }

    [Fact]
    public async Task ListStateAsync_WithoutPopulationEstimates_StillLoadsTheCities()
    {
        var source = Source(new Dictionary<string, string> { ["v1/localidades/estados/SP"] = Localities, ["v3/malhas/estados/SP"] = Boundaries });

        var cities = await source.ListStateAsync("SP", TestContext.Current.CancellationToken);

        Assert.All(cities, city => Assert.Equal(0, city.Population));
    }

    [Fact]
    public async Task ListStateAsync_SkipsAMunicipalityThatHasNoBoundaryYet()
    {
        var onlyJaboticabal = Boundaries.Replace("\"3550308\"", "\"9999999\"", StringComparison.Ordinal);
        var source = Source(new Dictionary<string, string> { ["v1/localidades/estados/SP"] = Localities, ["v3/malhas/estados/SP"] = onlyJaboticabal });

        var cities = await source.ListStateAsync("SP", TestContext.Current.CancellationToken);

        Assert.Equal(3524303, Assert.Single(cities).IbgeCode);
    }

    [Fact]
    public async Task ListStateAsync_WhenTheStateHasNoBoundary_Fails()
    {
        var source = Source(new Dictionary<string, string>
        {
            ["v1/localidades/estados/SP"] = Localities,
            ["v3/malhas/estados/SP"] = """{"type":"FeatureCollection","features":[]}"""
        });

        await Assert.ThrowsAsync<HttpRequestException>(() => source.ListStateAsync("SP", TestContext.Current.CancellationToken));
    }

    private static IbgeCitySource Source(IReadOnlyDictionary<string, string> routes) =>
        new(new HttpClient(new RoutedHttpHandler(routes)) { BaseAddress = new Uri("https://servicodados.ibge.gov.br/api/") }, NullLogger<IbgeCitySource>.Instance);
}
