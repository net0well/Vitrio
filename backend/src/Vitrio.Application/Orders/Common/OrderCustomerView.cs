using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderCustomerView(
    Guid Id,
    OrderStatus Status,
    string Title,
    string Description,
    string ServiceTitle,
    int CategoryId,
    DateOnly PreferredDay,
    PreferredPeriod PreferredPeriod,
    string Neighborhood,
    string CityName,
    DateTimeOffset CreatedAt,
    DateTimeOffset RespondBy,
    OrderProviderCard Provider,
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
    public static OrderCustomerView From(Order order, OrderProviderCard provider, IReadOnlyList<OrderPhotoResponse> photos, DateTimeOffset now) => new(
        order.Id,
        order.Status,
        order.Title,
        order.Description,
        order.ServiceTitle,
        order.CategoryId,
        order.PreferredDay,
        order.PreferredPeriod,
        order.Place.Neighborhood,
        order.Place.CityName,
        CityInstant.In(order.CreatedAt, order.Place.TimeZoneId),
        CityInstant.In(order.RespondBy, order.Place.TimeZoneId),
        provider,
        order is { Quote: { } quote, QuoteValidUntil: { } validUntil } ? OrderQuoteResponse.From(quote, validUntil, order.Place.TimeZoneId) : null,
        order.Schedule is { } schedule ? ServiceTimeResponse.From(schedule) : null,
        order.RescheduleCount,
        CityInstant.In(order.CompletedAt, order.Place.TimeZoneId),
        order.CompletedBy,
        order.DeclineReason is { } reason ? new OrderDeclineResponse(reason, order.DeclineDetails) : null,
        OrderCancellationResponse.For(order, OrderParty.Customer),
        photos,
        order.PersonalDataErasedAt is not null,
        order.AllowedActionsFor(OrderParty.Customer, now));
}
