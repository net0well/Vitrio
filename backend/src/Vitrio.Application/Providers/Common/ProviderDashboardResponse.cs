using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderDashboardResponse(
    ProviderStatus Status,
    int ProfileCompleteness,
    string NextStep,
    MeiStatus? MeiStatus,
    bool AvailableToday,
    int NewOrders,
    int ServicesThisMonth,
    IReadOnlyList<OrderProviderSummary> LatestNewOrders)
{
    public static ProviderDashboardResponse From(ProviderDashboardProfile profile, ProviderOrderNumbers orders) => new(
        profile.Status,
        profile.ProfileCompleteness,
        profile.NextStep,
        profile.MeiStatus,
        profile.AvailableToday,
        orders.NewOrders,
        orders.ServicesThisMonth,
        orders.LatestNewOrders);
}
