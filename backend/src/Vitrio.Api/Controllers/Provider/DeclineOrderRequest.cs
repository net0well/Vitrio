using Vitrio.Application.Orders.DeclineOrder;
using Vitrio.Domain.Orders;

namespace Vitrio.Api.Controllers.Provider;

public sealed record DeclineOrderRequest(DeclineReason? Reason, string? Details)
{
    public DeclineOrderCommand ToCommand(Guid orderId) => new(orderId, Reason, Details);
}
