using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Common;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.EraseOrderPersonalData;

public sealed class EraseOrderPersonalDataCommandHandler(
    IOrderWritableRepository orders,
    IPrivateMediaStorage privateMedia,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<EraseOrderPersonalDataCommand, Result>
{
    public async ValueTask<Result> Handle(EraseOrderPersonalDataCommand command, CancellationToken cancellationToken)
    {
        var endedBefore = clock.UtcNow - Order.PersonalDataKeptFor;
        await Batches.DrainAsync(
            (excluded, token) => orders.ListWithPersonalDataEndedBeforeAsync(endedBefore, excluded, command.BatchSize, token),
            EraseAsync,
            cancellationToken);
        return Result.Success();
    }

    private async Task<bool> EraseAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(orderId, cancellationToken);
        if (order is null || !order.CanErasePersonalData(clock.UtcNow))
        {
            return false;
        }

        // Files go before the save: once the record is saved without the keys, a failed delete would leave the photos
        // behind with nothing pointing at them.
        foreach (var objectKey in order.ErasePersonalData(clock))
        {
            await privateMedia.DeleteAsync(objectKey, cancellationToken);
        }

        auditLog.Record(OrderAudit.PersonalDataErased, null, OrderAudit.Target, order.Id);
        return await unitOfWork.TrySaveChangesAsync(cancellationToken);
    }
}
