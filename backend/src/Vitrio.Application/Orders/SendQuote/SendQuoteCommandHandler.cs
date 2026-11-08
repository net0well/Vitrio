using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.SendQuote;

public sealed class SendQuoteCommandHandler(
    IProviderOrderService providerOrders,
    IProviderOrderViewService views,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SendQuoteCommand, Result<OrderProviderView>>
{
    public async ValueTask<Result<OrderProviderView>> Handle(SendQuoteCommand command, CancellationToken cancellationToken)
    {
        var order = await providerOrders.LoadAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var terms = new QuoteTerms(
            command.Type!.Value,
            command.Amount!.Value,
            command.Date!.Value,
            OrderTimes.ParseStart(command.Start)!.Value,
            command.DurationMinutes!.Value,
            command.MaterialIncluded,
            command.MaterialDetails,
            command.Message);
        var sent = order.SendQuote(terms, clock);
        if (sent.IsFailure)
        {
            return sent.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var view = await views.GetAsync(order.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return view;
    }
}
