using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderProviderSummary(
    Guid Id,
    OrderStatus Status,
    string Title,
    string ServiceTitle,
    string Description,
    string CustomerName,
    string Neighborhood,
    double? DistanceKm,
    DateOnly PreferredDay,
    PreferredPeriod PreferredPeriod,
    DateTimeOffset CreatedAt,
    DateTimeOffset RespondBy,
    ServiceTimeResponse? Schedule,
    DateTimeOffset? QuoteValidUntil,
    OrderParty? CancelledBy,
    int PhotoCount);
