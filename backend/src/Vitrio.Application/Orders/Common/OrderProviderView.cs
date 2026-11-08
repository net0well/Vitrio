using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderProviderView(
    Guid Id,
    OrderStatus Status,
    string Title,
    string Description,
    string ServiceTitle,
    DateOnly PreferredDay,
    PreferredPeriod PreferredPeriod,
    DateTimeOffset CreatedAt,
    DateTimeOffset RespondBy,
    OrderCustomerCard Customer,
    string Neighborhood,
    string CityName,
    double? DistanceKm,
    OrderAddressResponse? Address,
    DateTimeOffset? ContactVisibleUntil,
    OrderQuoteResponse? Quote,
    ServiceTimeResponse? Schedule,
    int RescheduleCount,
    DateTimeOffset? CompletedAt,
    OrderParty? CompletedBy,
    OrderDeclineResponse? Decline,
    OrderCancellationResponse? Cancellation,
    IReadOnlyList<OrderPhotoResponse> Photos,
    bool PhotosRemoved,
    IReadOnlyList<OrderAction> AllowedActions)
{
    public static OrderProviderView From(Order order, OrderProviderContext context, IReadOnlyList<OrderPhotoResponse> photos, DateTimeOffset now)
    {
        var visible = order.IsContactVisibleToProvider(now);
        var address = visible ? order.Address : null;
        var contact = visible ? order.Contact : null;
        return new OrderProviderView(
            order.Id,
            order.Status,
            order.Title,
            order.Description,
            order.ServiceTitle,
            order.PreferredDay,
            order.PreferredPeriod,
            order.CreatedAt,
            order.RespondBy,
            new OrderCustomerCard(order.CustomerDisplayName, contact?.FullName, contact?.Phone, context.CustomerSince, context.CustomerOrderCount),
            order.Place.Neighborhood,
            order.Place.CityName,
            context.DistanceKm,
            address is null ? null : OrderAddressResponse.From(address, order.Place),
            contact is null ? null : order.ContactVisibleUntil,
            order is { Quote: { } quote, QuoteValidUntil: { } validUntil } ? OrderQuoteResponse.From(quote, validUntil) : null,
            order.Schedule is { } schedule ? ServiceTimeResponse.From(schedule) : null,
            order.RescheduleCount,
            order.CompletedAt,
            order.CompletedBy,
            order.DeclineReason is { } reason ? new OrderDeclineResponse(reason, order.DeclineDetails) : null,
            OrderCancellationResponse.For(order, OrderParty.Provider),
            photos,
            order.PersonalDataErasedAt is not null,
            order.AllowedActionsFor(OrderParty.Provider, now));
    }
}
