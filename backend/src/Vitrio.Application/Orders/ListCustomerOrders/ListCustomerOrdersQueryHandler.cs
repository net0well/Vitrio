using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.ListCustomerOrders;

public sealed class ListCustomerOrdersQueryHandler(ICurrentUser currentUser, IOrderSearchableRepository orders)
    : IQueryHandler<ListCustomerOrdersQuery, Result<OrderCustomerPage>>
{
    public async ValueTask<Result<OrderCustomerPage>> Handle(ListCustomerOrdersQuery query, CancellationToken cancellationToken) =>
        await orders.ListForCustomerAsync(
            currentUser.Id,
            query.Status == CustomerOrderFilter.Open,
            (query.Page - 1) * ListCustomerOrdersQuery.PageSize,
            ListCustomerOrdersQuery.PageSize,
            cancellationToken);
}
