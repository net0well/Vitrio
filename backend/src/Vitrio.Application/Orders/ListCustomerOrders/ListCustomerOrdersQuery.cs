using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.ListCustomerOrders;

public sealed record ListCustomerOrdersQuery(CustomerOrderFilter Status, int Page) : IQuery<Result<OrderCustomerPage>>
{
    public const int PageSize = 20;
    public const int MaxPage = 50;
}
