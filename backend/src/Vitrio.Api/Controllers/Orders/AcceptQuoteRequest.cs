using Vitrio.Application.Orders.AcceptQuote;

namespace Vitrio.Api.Controllers.Orders;

public sealed record AcceptQuoteRequest(int? QuoteVersion)
{
    public AcceptQuoteCommand ToCommand(Guid orderId) => new(orderId, QuoteVersion ?? 0);
}
