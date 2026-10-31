using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Providers;
using Vitrio.Application.Locations.Common;

namespace Vitrio.Api.IntegrationTests.Search;

public sealed record SearchableProvider(GeoSpot Point)
{
    public static readonly CitySummary FarAway = new(FakeIbgeCitySource.SaoJoseCode, "São José", "SC");

    public string DisplayName { get; init; } = "Carlos Menezes";

    public string Headline { get; init; } = "Eletricista";

    public string About { get; init; } = ProviderApi.About;

    public int[] CategoryIds { get; init; } = [1];

    public CitySummary City { get; init; } = FarAway;

    public int RadiusKm { get; init; } = 15;

    public object[] Services { get; init; } = [new { title = "Troca de tomada", priceFrom = 80m, priceUnit = "perService" }];
}
