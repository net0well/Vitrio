using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderPublicSnapshot(
    Guid Id,
    string DisplayName,
    string Headline,
    string? About,
    int? YearsOfExperience,
    string? AvatarUrl,
    CitySummary City,
    string TimeZone,
    IReadOnlyList<string> Neighborhoods,
    int RadiusKm,
    IReadOnlyList<ProviderCategoryResponse> Categories,
    IReadOnlyList<ProviderServiceResponse> Services,
    IReadOnlyList<AvailabilitySlotResponse> Availability,
    bool HandlesEmergencies,
    bool MeiVerified,
    decimal? RatingScore,
    int ReviewCount,
    string? ResponseTimeBucket,
    int CompletedOrders,
    string? WhatsAppPhone,
    double PublicLatitude,
    double PublicLongitude,
    bool AvailableToday,
    DateTimeOffset? AvailableTodaySetAt,
    IReadOnlyList<PublicPhotoResponse> FirstPhotos)
{
    public bool IsRated(int minimumReviews) => ReviewCount >= minimumReviews && RatingScore is not null;
}
