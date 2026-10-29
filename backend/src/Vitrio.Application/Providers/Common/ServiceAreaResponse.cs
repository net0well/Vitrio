using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Providers.Common;

public sealed record ServiceAreaResponse(
    string Cep,
    string? Street,
    string Number,
    string? Complement,
    string? Neighborhood,
    CitySummary City,
    int RadiusKm,
    IReadOnlyList<string> Neighborhoods,
    bool LocationApproximate);
