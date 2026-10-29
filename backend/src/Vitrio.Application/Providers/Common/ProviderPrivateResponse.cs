using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderPrivateResponse(
    Guid Id,
    ProviderStatus Status,
    string DisplayName,
    string Headline,
    string? About,
    int? YearsOfExperience,
    string? AvatarUrl,
    string? ContactPhone,
    bool WhatsAppEnabled,
    bool HandlesEmergencies,
    bool AvailableToday,
    IReadOnlyList<ProviderCategoryResponse> Categories,
    ServiceAreaResponse? ServiceArea,
    IReadOnlyList<ProviderServiceResponse> Services,
    IReadOnlyList<AvailabilitySlotResponse> Availability,
    IReadOnlyList<string> Missing);
