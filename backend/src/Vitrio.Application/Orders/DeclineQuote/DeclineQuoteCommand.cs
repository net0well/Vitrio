using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.DeclineQuote;

public sealed record DeclineQuoteCommand(Guid OrderId) : ICommand<Result<OrderCustomerView>>;
