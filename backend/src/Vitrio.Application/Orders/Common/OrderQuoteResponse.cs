using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderQuoteResponse(
    QuoteType Type,
    decimal Amount,
    ServiceTimeResponse Time,
    bool MaterialIncluded,
    string? MaterialDetails,
    string? Message,
    DateTimeOffset SentAt,
    DateTimeOffset ValidUntil,
    int Version)
{
    public static OrderQuoteResponse From(Quote quote, DateTimeOffset validUntil, string timeZoneId) => new(
        quote.Type,
        quote.Amount.Reais,
        ServiceTimeResponse.From(quote.Schedule),
        quote.MaterialIncluded,
        quote.MaterialDetails,
        quote.Message,
        CityInstant.In(quote.SentAt, timeZoneId),
        CityInstant.In(validUntil, timeZoneId),
        quote.Version);
}
