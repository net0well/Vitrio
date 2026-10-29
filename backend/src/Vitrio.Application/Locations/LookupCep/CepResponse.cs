using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Locations.LookupCep;

public sealed record CepResponse(string Cep, string? Street, string? Neighborhood, CitySummary City);
