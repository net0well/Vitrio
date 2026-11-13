namespace Vitrio.Application.Orders.Common;

public sealed record AgendaItemResponse(
    Guid OrderId,
    AgendaItemKind Kind,
    DateOnly Date,
    string Start,
    string End,
    string Title,
    string CustomerName,
    string Neighborhood,
    DateTimeOffset? QuoteValidUntil);
