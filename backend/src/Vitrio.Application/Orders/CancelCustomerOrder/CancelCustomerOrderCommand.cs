using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CancelCustomerOrder;

public sealed record CancelCustomerOrderCommand(Guid OrderId, CancellationReason? Reason, string? Details) : ICommand<Result<OrderCustomerView>>;
