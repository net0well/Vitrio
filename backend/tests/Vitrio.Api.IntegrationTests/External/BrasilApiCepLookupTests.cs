using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Infrastructure.External.BrasilApi;

namespace Vitrio.Api.IntegrationTests.External;

public sealed class BrasilApiCepLookupTests
{
    private static readonly Cep Cep = Vitrio.Domain.Accounts.Cep.Create("14870000").Value;

    [Fact]
    public async Task FindAsync_ReadsStreetCityStateAndCoordinates()
    {
        var handler = new StubHttpHandler(HttpStatusCode.OK, """
            {"cep":"14870000","state":"SP","city":"Jaboticabal","neighborhood":"Centro","street":"Rua Rui Barbosa","service":"open-cep",
             "location":{"type":"Point","coordinates":{"longitude":"-48.3221","latitude":"-21.2547"}}}
            """);

        var result = await Lookup(handler).FindAsync(Cep, TestContext.Current.CancellationToken);

        var found = result.Value!;
        Assert.Equal("Rua Rui Barbosa", found.Street);
        Assert.Equal("Jaboticabal", found.City);
        Assert.Equal("SP", found.State);
        Assert.Equal(-21.2547, found.Latitude);
        Assert.Equal(-48.3221, found.Longitude);
        Assert.EndsWith("/api/cep/v2/14870000", handler.Requests.Single().AbsolutePath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FindAsync_WithoutCoordinatesOrStreet_KeepsThemEmpty()
    {
        var handler = new StubHttpHandler(HttpStatusCode.OK, """{"cep":"14870000","state":"SP","city":"Jaboticabal","neighborhood":null,"street":null,"location":{"type":"Point","coordinates":{}}}""");

        var found = (await Lookup(handler).FindAsync(Cep, TestContext.Current.CancellationToken)).Value!;

        Assert.Null(found.Street);
        Assert.Null(found.Latitude);
    }

    [Fact]
    public async Task FindAsync_WhenTheCepDoesNotExist_ReturnsNothing()
    {
        var handler = new StubHttpHandler(HttpStatusCode.NotFound, """{"name":"CepPromiseError","message":"Todos os serviços de CEP retornaram erro."}""");

        var result = await Lookup(handler).FindAsync(Cep, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task FindAsync_WhenTheServiceFails_ReturnsUnavailable()
    {
        var handler = new StubHttpHandler(HttpStatusCode.InternalServerError, "{}");

        var result = await Lookup(handler).FindAsync(Cep, TestContext.Current.CancellationToken);

        Assert.Equal(AccountErrors.CepUnavailable, result.Error);
    }

    private static BrasilApiCepLookup Lookup(StubHttpHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://brasilapi.com.br/api/") }, NullLogger<BrasilApiCepLookup>.Instance);
}
