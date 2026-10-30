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

    public string CityName { get; init; } = string.Empty;

    public double? DistanceMeters { get; init; }

    public bool MeiVerified { get; init; }

    public bool AvailableToday { get; init; }

    public long? PriceFromCents { get; init; }

    public string? PriceUnit { get; init; }

    public double Relevance { get; init; }
}
