using Mediator;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.SendQuote;

public sealed record SendQuoteCommand(
    Guid OrderId,
    QuoteType? Type,
    decimal? Amount,
    DateOnly? Date,
    string? Start,
    int? DurationMinutes,
    bool MaterialIncluded,
    string? MaterialDetails,
    string? Message,
    Guid IdempotencyKey,
    string RequestHash) : ICommand<Result<OrderProviderView>>, IIdempotentCommand;
