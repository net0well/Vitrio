using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Search.Common;

public sealed record SearchContextResponse(SearchMode Mode, CitySummary? City, ProfessionalNameResponse? Professional)
{
    public static SearchContextResponse From(SearchArea area, ProfessionalNameResponse? professional) => new(area.Mode, area.City, professional);
}
