using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.AcceptQuote;

public sealed record AcceptQuoteCommand(Guid OrderId, int QuoteVersion) : ICommand<Result<OrderCustomerView>>;
