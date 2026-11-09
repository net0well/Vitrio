using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.ExpireOrders;

public sealed class ExpireOrdersCommandHandler(IOrderWritableRepository orders, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<ExpireOrdersCommand, Result>
{
    public async ValueTask<Result> Handle(ExpireOrdersCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        await Batches.DrainAsync(
            (excluded, token) => orders.ListDueToExpireAsync(now, excluded, command.BatchSize, token),
            ExpireAsync,
            cancellationToken);
        return Result.Success();
    }

    private async Task<bool> ExpireAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(orderId, cancellationToken);
        return order is not null && order.Expire(clock) && await unitOfWork.TrySaveChangesAsync(cancellationToken);
    }
}
