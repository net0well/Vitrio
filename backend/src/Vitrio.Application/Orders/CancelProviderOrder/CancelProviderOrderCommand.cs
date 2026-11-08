using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CancelProviderOrder;

public sealed record CancelProviderOrderCommand(Guid OrderId, CancellationReason? Reason, string? Details) : ICommand<Result<OrderProviderView>>;
