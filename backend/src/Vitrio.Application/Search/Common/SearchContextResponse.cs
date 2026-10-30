using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Search.Common;

public sealed record SearchContextResponse(SearchMode Mode, CitySummary? City)
{
    public static SearchContextResponse From(SearchArea area) => new(area.Mode, area.City);
}
