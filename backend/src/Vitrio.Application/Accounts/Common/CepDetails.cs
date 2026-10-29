using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Accounts.Common;

public sealed record CepDetails(string? Street, string? Neighborhood, CitySummary City, double? Latitude, double? Longitude);
