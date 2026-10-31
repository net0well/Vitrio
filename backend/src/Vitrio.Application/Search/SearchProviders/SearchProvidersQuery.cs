using Mediator;
using Vitrio.Application.Search.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Search.SearchProviders;

public sealed record SearchProvidersQuery(
    string? Q,
    int? CategoryId,
    double? Latitude,
    double? Longitude,
    int? CityIbgeCode,
    int? MaxDistanceKm,
    decimal? MinRating,
    bool MeiVerified,
    bool AvailableToday,
    decimal? MaxPriceFrom,
    SearchSort Sort,
    int Page,
    int PageSize) : IQuery<Result<ProviderSearchPage>>, ISearchLocation
{
    public const int MaxPage = 10;
    public const int MaxPageSize = 30;
    public const int DefaultPageSize = 20;
    public const int MaxTermLength = 60;
    public const int MaxCount = 200;

    public override string ToString() => nameof(SearchProvidersQuery);
}
