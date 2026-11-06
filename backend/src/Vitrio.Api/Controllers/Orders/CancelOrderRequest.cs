using Vitrio.Application.Orders.CancelCustomerOrder;
using Vitrio.Domain.Orders;

namespace Vitrio.Api.Controllers.Orders;

public sealed record CancelOrderRequest(CancellationReason? Reason, string? Details)
{
    public CancelCustomerOrderCommand ToCustomerCommand(Guid orderId) => new(orderId, Reason, Details);
}
