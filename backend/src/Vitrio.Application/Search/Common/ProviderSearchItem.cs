using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Search.Common;

public sealed record ProviderSearchItem(
    Guid Id,
    string DisplayName,
    string Headline,
    string Initials,
    string? AvatarUrl,
    ProviderRatingResponse? Rating,
    bool IsNew,
    string CityName,
    double? DistanceKm,
    ProviderBadgesResponse Badges,
    string? ResponseTime,
    ProviderPriceResponse? Price);
