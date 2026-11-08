using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.DeclineOrder;

public sealed record DeclineOrderCommand(Guid OrderId, DeclineReason? Reason, string? Details) : ICommand<Result<OrderProviderView>>;
