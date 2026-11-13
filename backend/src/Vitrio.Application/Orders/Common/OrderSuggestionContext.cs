using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderSuggestionContext(OrderStatus Status, Guid ProviderId, int CategoryId, double Latitude, double Longitude)
{
    public bool AllowsSuggestions => Status is OrderStatus.Declined or OrderStatus.Expired;
}
