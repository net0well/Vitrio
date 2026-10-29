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

    [Fact]
    public async Task ListStateAsync_JoinsNamesAndBoundariesByIbgeCode()
    {
        var source = Source(new Dictionary<string, string> { ["v1/localidades/estados/SP"] = Localities, ["v3/malhas/estados/SP"] = Boundaries });

        var cities = await source.ListStateAsync("SP", TestContext.Current.CancellationToken);

        Assert.Equal([3524303, 3550308], cities.Select(city => city.IbgeCode));
        Assert.Equal("São Paulo", cities[1].Name);
        Assert.Contains("MultiPolygon", cities[1].BoundaryGeoJson, StringComparison.Ordinal);
        Assert.All(cities, city => Assert.Equal("SP", city.State));
    }

    [Fact]
    public async Task ListStateAsync_WhenBoundariesAreMissing_Fails()
    {
        var source = Source(new Dictionary<string, string>
        {
            ["v1/localidades/estados/SP"] = Localities,
            ["v3/malhas/estados/SP"] = """{"type":"FeatureCollection","features":[]}"""
        });

        await Assert.ThrowsAsync<HttpRequestException>(() => source.ListStateAsync("SP", TestContext.Current.CancellationToken));
    }

    private static IbgeCitySource Source(IReadOnlyDictionary<string, string> routes) =>
        new(new HttpClient(new RoutedHttpHandler(routes)) { BaseAddress = new Uri("https://servicodados.ibge.gov.br/api/") });
}
