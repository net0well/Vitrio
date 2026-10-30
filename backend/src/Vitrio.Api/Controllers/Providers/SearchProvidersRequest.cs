using Vitrio.Application.Search.Common;
using Vitrio.Application.Search.SearchProviders;

namespace Vitrio.Api.Controllers.Providers;

public sealed record SearchProvidersRequest(
    string? Q,
    int? CategoryId,
    double? Lat,
    double? Lng,
    int? CityIbgeCode,
    int? MaxDistanceKm,
    decimal? MinRating,
    bool? MeiVerified,
    bool? AvailableToday,
    decimal? MaxPriceFrom,
    SearchSort? Sort,
    int? Page,
    int? PageSize)
{
    public SearchProvidersQuery ToQuery() => new(
        Q,
        CategoryId,
        Lat,
        Lng,
        CityIbgeCode,
        MaxDistanceKm,
        MinRating,
        MeiVerified ?? false,
        AvailableToday ?? false,
        MaxPriceFrom,
        Sort ?? SearchSort.Relevance,
        Page ?? 1,
        PageSize ?? SearchProvidersQuery.DefaultPageSize);
}
