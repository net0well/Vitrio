using Mediator;
using Vitrio.Application.Locations.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Locations.SearchCities;

public sealed record SearchCitiesQuery(string Search, string? State, int? Limit) : IQuery<Result<IReadOnlyList<CitySummary>>>
{
    public const int DefaultLimit = 10;
    public const int MaxLimit = 20;
}
