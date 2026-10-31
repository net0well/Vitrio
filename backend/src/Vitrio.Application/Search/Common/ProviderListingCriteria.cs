namespace Vitrio.Application.Search.Common;

public sealed record ProviderListingCriteria(
    SearchArea Area,
    string? Term,
    int? CategoryId,
    double MaxDistanceMeters,
    decimal? MinRating,
    bool MeiVerified,
    bool AvailableToday,
    long? MaxPriceFromCents,
    SearchSort Sort,
    int Skip,
    int Take,
    DateTimeOffset Now);
