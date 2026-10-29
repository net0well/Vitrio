using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderPublicResponse(
    Guid Id,
    string DisplayName,
    string Headline,
    string? About,
    int? YearsOfExperience,
    string? AvatarUrl,
    CitySummary City,
    IReadOnlyList<string> Neighborhoods,
    int RadiusKm,
    IReadOnlyList<ProviderCategoryResponse> Categories,
    IReadOnlyList<ProviderServiceResponse> Services,
    IReadOnlyList<AvailabilitySlotResponse> Availability,
    bool HandlesEmergencies,
    ProviderBadgesResponse Badges,
    ProviderRatingResponse? Rating,
    bool IsNew,
    string? ResponseTime,
    int? CompletedOrders,
    string? WhatsAppPhone,
    double? DistanceKm,
    IReadOnlyList<PublicPhotoResponse> FirstPhotos,
    string ShareUrl);
