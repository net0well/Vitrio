namespace Vitrio.Infrastructure.Persistence.Search;

internal sealed class ProviderListingRow
{
    public Guid Id { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string Headline { get; init; } = string.Empty;

    public string? AvatarKey { get; init; }

    public decimal? RatingScore { get; init; }

    public int ReviewCount { get; init; }

    public string? ResponseTimeBucket { get; init; }

    public double? PublicLatitude { get; init; }

    public double? PublicLongitude { get; init; }

    public int ServiceRadiusMeters { get; init; }

    public bool AvailableTodaySwitch { get; init; }

    public DateTimeOffset? AvailableTodaySetAt { get; init; }

    public long? ServicePriceCents { get; init; }

    public string CityName { get; init; } = string.Empty;

    public string TimeZone { get; init; } = string.Empty;

    public double? DistanceMeters { get; init; }

    public DateTimeOffset? SavedAt { get; init; }

    public bool MeiVerified { get; init; }

    public bool AvailableToday { get; init; }

    public long? PriceFromCents { get; init; }

    public string? PriceUnit { get; init; }

    public double Relevance { get; init; }
}
