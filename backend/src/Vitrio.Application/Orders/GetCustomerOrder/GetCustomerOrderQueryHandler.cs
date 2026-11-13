using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Services;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.GetCustomerOrder;

public sealed class GetCustomerOrderQueryHandler(ICustomerOrderViewService views) : IQueryHandler<GetCustomerOrderQuery, Result<OrderCustomerView>>
{
    public async ValueTask<Result<OrderCustomerView>> Handle(GetCustomerOrderQuery query, CancellationToken cancellationToken) =>
        await views.GetAsync(query.OrderId, cancellationToken);
}
