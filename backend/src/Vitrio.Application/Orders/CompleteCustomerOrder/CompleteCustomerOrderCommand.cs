using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CompleteCustomerOrder;

public sealed record CompleteCustomerOrderCommand(Guid OrderId) : ICommand<Result<OrderCustomerView>>;
