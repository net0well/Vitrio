using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Providers.Common;

public sealed record ServiceAreaSavedResponse(CitySummary City, string? Neighborhood, bool LocationApproximate);
