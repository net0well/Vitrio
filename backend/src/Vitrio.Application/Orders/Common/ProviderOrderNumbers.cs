namespace Vitrio.Application.Orders.Common;

public sealed record ProviderOrderNumbers(int NewOrders, int ServicesThisMonth, IReadOnlyList<OrderProviderSummary> LatestNewOrders)
{
    public static readonly ProviderOrderNumbers None = new(0, 0, []);
}
