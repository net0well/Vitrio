using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderCustomerSummary(
    Guid Id,
    string Title,
    int CategoryId,
    string ProviderName,
    OrderStatus Status,
    DateTimeOffset CreatedAt,
    ServiceTimeResponse? Schedule);
