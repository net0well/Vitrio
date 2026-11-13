using Vitrio.Application.Orders.RescheduleOrder;

namespace Vitrio.Api.Controllers.Provider;

public sealed record RescheduleOrderRequest(DateOnly? Date, string? Start, int? DurationMinutes)
{
    public RescheduleOrderCommand ToCommand(Guid orderId) => new(orderId, Date, Start, DurationMinutes);
}
