namespace Vitrio.Application.Orders.Common;

public sealed record OrderCustomerPage(IReadOnlyList<OrderCustomerSummary> Items, bool HasMore);
