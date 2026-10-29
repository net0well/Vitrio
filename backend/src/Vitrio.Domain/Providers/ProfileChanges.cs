using Vitrio.Domain.Accounts;

namespace Vitrio.Domain.Providers;

public sealed record ProfileChanges(
    DisplayName? DisplayName,
    string? Headline,
    string? About,
    int? YearsOfExperience,
    PhoneNumber? ContactPhone,
    bool? WhatsAppEnabled,
    bool? HandlesEmergencies);
