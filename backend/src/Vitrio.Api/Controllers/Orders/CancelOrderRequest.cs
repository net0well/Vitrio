using Vitrio.Application.Orders.CancelCustomerOrder;
using Vitrio.Application.Orders.CancelProviderOrder;
using Vitrio.Domain.Orders;

namespace Vitrio.Api.Controllers.Orders;

public sealed record CancelOrderRequest(CancellationReason? Reason, string? Details)
{
    public CancelCustomerOrderCommand ToCustomerCommand(Guid orderId) => new(orderId, Reason, Details);

    public CancelProviderOrderCommand ToProviderCommand(Guid orderId) => new(orderId, Reason, Details);
}
