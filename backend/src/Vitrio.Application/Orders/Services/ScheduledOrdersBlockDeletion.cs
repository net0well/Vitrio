using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.Services;

internal sealed class ScheduledOrdersBlockDeletion(IOrderSearchableRepository orders) : IAccountDeletionBlocker
{
    public async Task<Error?> FindBlockerAsync(Guid userId, CancellationToken cancellationToken) =>
        await orders.HasScheduledForUserAsync(userId, cancellationToken) ? OrderErrors.ScheduledOrderBlocksDeletion : null;
}
