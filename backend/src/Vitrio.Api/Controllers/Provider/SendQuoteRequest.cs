using Vitrio.Application.Orders.SendQuote;
using Vitrio.Domain.Orders;

namespace Vitrio.Api.Controllers.Provider;

public sealed record SendQuoteRequest(
    QuoteType? Type,
    decimal? Amount,
    DateOnly? Date,
    string? Start,
    int? DurationMinutes,
    bool? MaterialIncluded,
    string? MaterialDetails,
    string? Message)
{
    public SendQuoteCommand ToCommand(Guid orderId, Guid? idempotencyKey, string requestHash) => new(
        orderId,
        Type,
        Amount,
        Date,
        Start,
        DurationMinutes,
        MaterialIncluded ?? false,
        MaterialDetails,
        Message,
        idempotencyKey ?? Guid.Empty,
        requestHash);
}
