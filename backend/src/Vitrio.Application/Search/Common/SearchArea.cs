using Vitrio.Application.Locations.Common;
using Vitrio.Domain.Locations;

namespace Vitrio.Application.Search.Common;

public sealed record SearchArea(SearchMode Mode, Coordinates? Point, CitySummary? City)
{
    public bool IsNearMe => Mode == SearchMode.NearMe;
}
