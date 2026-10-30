using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.UpdateProvider;

public sealed record UpdateProviderCommand(
    string? DisplayName,
    string? Headline,
    string? About,
    int? YearsOfExperience,
    string? ContactPhone,
    bool? WhatsAppEnabled,
    bool? HandlesEmergencies) : ICommand<Result<ProviderPrivateResponse>>;
