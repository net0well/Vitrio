using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderDashboardResponse(
    ProviderStatus Status,
    int ProfileCompleteness,
    string NextStep,
    string? MeiStatus,
    bool AvailableToday);
