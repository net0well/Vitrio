using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderDashboardProfile(
    Guid Id,
    ProviderStatus Status,
    int ProfileCompleteness,
    string NextStep,
    MeiStatus? MeiStatus,
    bool AvailableToday,
    string? TimeZone);
