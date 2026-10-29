using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.IntegrationTests.Accounts;

public sealed class AddressTests(ApiFactory factory)
{
    private const string Addresses = "/api/v1/me/addresses";

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task AddAddress_WithKnownCep_FillsStreetNeighborhoodCityAndState()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Addresses, auth.AccessToken, Body("14870-000"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>(TestContext.Current.CancellationToken);
        Assert.Equal("Rua Rui Barbosa", address!.Street);
        Assert.Equal("Centro", address.Neighborhood);
        Assert.Equal("Jaboticabal", address.City);
        Assert.Equal("SP", address.State);
        Assert.Equal(FakeIbgeCitySource.JaboticabalCode, address.CityIbgeCode);

        var list = await (await _client.SendAsAsync(HttpMethod.Get, Addresses, auth.AccessToken))
            .Content.ReadFromJsonAsync<List<AddressResponse>>(TestContext.Current.CancellationToken);
        Assert.Equal(address.Id, Assert.Single(list!).Id);
    }

    [Fact]
    public async Task AddAddress_WithUnknownCepAndNoDetails_ReturnsCepNotFound()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Addresses, auth.AccessToken, Body(FakeCepLookup.Unknown));

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "address.cep_not_found");
    }

    [Fact]
    public async Task AddAddress_WithUnknownCepAndManualDetails_UsesTheCityFromTheIbgeList()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        var body = Body(FakeCepLookup.Unknown) with
        {
            Street = "Rua Nova",
            Neighborhood = "Jardim das Flores",
            State = "SP",
            CityIbgeCode = FakeIbgeCitySource.SaoPauloCode
        };

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Addresses, auth.AccessToken, body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>(TestContext.Current.CancellationToken);
        Assert.Equal("São Paulo", address!.City);
        Assert.Equal("Rua Nova", address.Street);
    }

    [Fact]
    public async Task AddAddress_WithManualCityOutsideTheIbgeList_ReturnsCityNotFound()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        var body = Body(FakeCepLookup.Unknown) with { Street = "Rua Nova", Neighborhood = "Centro", State = "SP", CityIbgeCode = 3500000 };

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Addresses, auth.AccessToken, body);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "address.city_not_found");
    }

    [Fact]
    public async Task AddAddress_WhenTheCepServiceIsDown_ReturnsServiceUnavailable()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Addresses, auth.AccessToken, Body(FakeCepLookup.Unreachable));

        await response.AssertProblemAsync(HttpStatusCode.ServiceUnavailable, "address.cep_unavailable");
    }

    [Fact]
    public async Task AddAddress_BeyondTenAddresses_ReturnsConflict()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        for (var index = 0; index < Address.MaxPerUser; index++)
        {
            var created = await _client.SendJsonAsAsync(HttpMethod.Post, Addresses, auth.AccessToken, Body(FakeCepLookup.Jaboticabal) with { Label = $"Endereço {index}" });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        }

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Addresses, auth.AccessToken, Body(FakeCepLookup.Jaboticabal));

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "address.limit_reached");
    }

    [Fact]
    public async Task UpdateAddress_ChangesTheNumberAndLabel()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        var created = await CreateAsync(auth.AccessToken);

        var response = await _client.SendJsonAsAsync(
            HttpMethod.Put,
            $"{Addresses}/{created.Id}",
            auth.AccessToken,
            Body(FakeCepLookup.Jaboticabal) with { Label = "Trabalho", Number = "455" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>(TestContext.Current.CancellationToken);
        Assert.Equal("Trabalho", address!.Label);
        Assert.Equal("455", address.Number);
    }

    [Fact]
    public async Task UpdateOrDeleteAddress_OfAnotherPerson_ReturnsNotFound()
    {
        var owner = await factory.SignInNewAccountAsync(_client);
        var created = await CreateAsync(owner.AccessToken);
        var stranger = await factory.SignInNewAccountAsync(_client);

        var update = await _client.SendJsonAsAsync(HttpMethod.Put, $"{Addresses}/{created.Id}", stranger.AccessToken, Body(FakeCepLookup.Jaboticabal));
        var delete = await _client.SendAsAsync(HttpMethod.Delete, $"{Addresses}/{created.Id}", stranger.AccessToken);

        await update.AssertProblemAsync(HttpStatusCode.NotFound, "address.not_found");
        await delete.AssertProblemAsync(HttpStatusCode.NotFound, "address.not_found");
        var list = await (await _client.SendAsAsync(HttpMethod.Get, Addresses, owner.AccessToken))
            .Content.ReadFromJsonAsync<List<AddressResponse>>(TestContext.Current.CancellationToken);
        Assert.Single(list!);
    }

    [Fact]
    public async Task DeleteAddress_RemovesIt()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        var created = await CreateAsync(auth.AccessToken);

        var response = await _client.SendAsAsync(HttpMethod.Delete, $"{Addresses}/{created.Id}", auth.AccessToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var list = await (await _client.SendAsAsync(HttpMethod.Get, Addresses, auth.AccessToken))
            .Content.ReadFromJsonAsync<List<AddressResponse>>(TestContext.Current.CancellationToken);
        Assert.Empty(list!);
    }

    [Fact]
    public async Task Address_IsEncryptedInTheDatabase()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        var created = await CreateAsync(auth.AccessToken);

        var stored = await factory.QueryAsync(db => db.Database
            .SqlQuery<string>($"SELECT street || '|' || number || '|' || cep AS \"Value\" FROM addresses WHERE id = {created.Id}")
            .SingleAsync(TestContext.Current.CancellationToken));

        Assert.DoesNotContain("Rui Barbosa", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("14870000", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("|120|", stored, StringComparison.Ordinal);
    }

    private async Task<AddressResponse> CreateAsync(string accessToken)
    {
        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Addresses, accessToken, Body(FakeCepLookup.Jaboticabal));
        return (await response.Content.ReadFromJsonAsync<AddressResponse>(TestContext.Current.CancellationToken))!;
    }

    private static AddressBody Body(string cep) => new("Casa", cep, "120", "ap 31", null, null, null, null);

    private sealed record AddressBody(
        string Label,
        string Cep,
        string Number,
        string? Complement,
        string? Street,
        string? Neighborhood,
        string? State,
        int? CityIbgeCode);
}
