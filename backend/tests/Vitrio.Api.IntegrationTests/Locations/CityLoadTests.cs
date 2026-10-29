using System.Net.Http.Json;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.LoadCities;

namespace Vitrio.Api.IntegrationTests.Locations;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class CityLoadTests(ApiFactory factory)
{
    [Fact]
    public async Task LoadCities_RunTwice_KeepsOneRowPerCityAndRefreshesTheSearch()
    {
        var client = factory.CreateClient();
        var before = await client.GetFromJsonAsync<List<CitySummary>>("/api/v1/cities?search=jaboticabal", TestContext.Current.CancellationToken);
        Assert.Equal("Jaboticabal", Assert.Single(before!).Name);

        factory.Ibge.Renamed[FakeIbgeCitySource.JaboticabalCode] = "Jaboticabal Teste";
        try
        {
            var loaded = await SendAsync(new LoadCitiesCommand(["SP", "SC"]));

            Assert.Equal(5, loaded);
            var count = await CountAsync(FakeIbgeCitySource.JaboticabalCode);
            Assert.Equal(1, count);
            var after = await client.GetFromJsonAsync<List<CitySummary>>("/api/v1/cities?search=jaboticabal", TestContext.Current.CancellationToken);
            Assert.Equal("Jaboticabal Teste", Assert.Single(after!).Name);
        }
        finally
        {
            factory.Ibge.Renamed.Clear();
            await SendAsync(new LoadCitiesCommand(["SP", "SC"]));
        }
    }

    [Fact]
    public async Task LoadCities_WithAnUnknownState_FailsValidationWithoutCallingTheIbge()
    {
        await using var scope = factory.Services.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<ISender>()
            .Send(new LoadCitiesCommand(["SP", "XX"]), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("validation", result.Error!.Code);
    }

    private async Task<int> SendAsync(LoadCitiesCommand command)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return (await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, TestContext.Current.CancellationToken)).Value;
    }

    private async Task<int> CountAsync(int ibgeCode)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Vitrio.Infrastructure.Persistence.AppDbContext>();
        return await db.Cities.CountAsync(city => city.IbgeCode == ibgeCode, TestContext.Current.CancellationToken);
    }
}
