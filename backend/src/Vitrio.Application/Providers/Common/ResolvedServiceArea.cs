using Vitrio.Application.Locations.Common;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ResolvedServiceArea(BaseAddress Address, Coordinates Point, bool Approximate, CitySummary City);
