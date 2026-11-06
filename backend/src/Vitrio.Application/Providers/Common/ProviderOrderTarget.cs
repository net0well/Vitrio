using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderOrderTarget(
    Guid Id,
    Guid UserId,
    ProviderStatus Status,
    bool OwnerActive,
    int? PrimaryCategoryId,
    IReadOnlyDictionary<Guid, string> Services)
{
    public bool IsVisible => Status is ProviderStatus.Published or ProviderStatus.Paused or ProviderStatus.Suspended;

    public bool ReceivesOrders => Status == ProviderStatus.Published && OwnerActive;
}
