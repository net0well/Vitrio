using Vitrio.Application.Providers.UpdateProvider;

namespace Vitrio.Api.Controllers.Provider;

public sealed record UpdateProviderRequest(
    string? DisplayName,
    string? Headline,
    string? About,
    int? YearsOfExperience,
    string? ContactPhone,
    bool? WhatsAppEnabled,
    bool? HandlesEmergencies)
{
    public UpdateProviderCommand ToCommand() =>
        new(DisplayName, Headline, About, YearsOfExperience, ContactPhone, WhatsAppEnabled, HandlesEmergencies);
}
