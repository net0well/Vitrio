namespace Vitrio.Application.Orders.Common;

public sealed record OrderProviderPage(IReadOnlyList<OrderProviderSummary> Items, bool HasMore);
