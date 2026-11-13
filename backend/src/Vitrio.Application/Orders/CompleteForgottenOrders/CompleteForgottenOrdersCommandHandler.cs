using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.CompleteForgottenOrders;

public sealed class CompleteForgottenOrdersCommandHandler(IOrderWritableRepository orders, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CompleteForgottenOrdersCommand, Result>
{
    public async ValueTask<Result> Handle(CompleteForgottenOrdersCommand command, CancellationToken cancellationToken)
    {
        var endedBefore = clock.UtcNow - Order.AutomaticCompletionAfter;
        await Batches.DrainAsync(
            (excluded, token) => orders.ListScheduledEndedBeforeAsync(endedBefore, excluded, command.BatchSize, token),
            CompleteAsync,
            cancellationToken);
        return Result.Success();
    }

    private async Task<bool> CompleteAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(orderId, cancellationToken);
        return order is not null && order.CompleteForgotten(clock) && await unitOfWork.TrySaveChangesAsync(cancellationToken);
    }
}
