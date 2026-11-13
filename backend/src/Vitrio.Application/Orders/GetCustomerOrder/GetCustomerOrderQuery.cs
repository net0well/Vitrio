using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.GetCustomerOrder;

public sealed record GetCustomerOrderQuery(Guid OrderId) : IQuery<Result<OrderCustomerView>>;
