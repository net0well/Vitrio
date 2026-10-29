using System.Net;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Infrastructure.External.Ibge;

namespace Vitrio.Api.IntegrationTests.External;

public sealed class IbgeCityDirectoryTests
{
    [Fact]
    public async Task ListAsync_ReadsTheFlatViewAndCachesIt()
    {
        var handler = new StubHttpHandler(HttpStatusCode.OK, """
            [{"municipio-id":3524303,"municipio-nome":"Jaboticabal","UF-sigla":"SP"},
             {"municipio-id":3550308,"municipio-nome":"São Paulo","UF-sigla":"SP"}]
            """);
        var directory = Directory(handler);

        var first = await directory.ListAsync("SP", TestContext.Current.CancellationToken);
        var second = await directory.ListAsync("SP", TestContext.Current.CancellationToken);

        Assert.Equal([3524303, 3550308], first.Value.Select(city => city.Code));
        Assert.Equal("São Paulo", second.Value[1].Name);
        Assert.Single(handler.Requests);
        Assert.Contains("localidades/estados/SP/municipios", handler.Requests.Single().AbsolutePath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListAsync_WhenTheServiceFails_ReturnsUnavailableWithoutCaching()
    {
        var handler = new StubHttpHandler(HttpStatusCode.ServiceUnavailable, "{}");
        var directory = Directory(handler);

        var first = await directory.ListAsync("SP", TestContext.Current.CancellationToken);
        await directory.ListAsync("SP", TestContext.Current.CancellationToken);

        Assert.Equal(AccountErrors.CepUnavailable, first.Error);
        Assert.Equal(2, handler.Requests.Count);
    }

    private static IbgeCityDirectory Directory(StubHttpHandler handler)
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        var cache = services.BuildServiceProvider().GetRequiredService<HybridCache>();
        return new IbgeCityDirectory(
            new HttpClient(handler) { BaseAddress = new Uri("https://servicodados.ibge.gov.br/api/v1/") },
            cache,
            NullLogger<IbgeCityDirectory>.Instance);
    }
}
