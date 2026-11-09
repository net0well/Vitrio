using Mediator;
using Vitrio.Application.Orders.CompleteForgottenOrders;
using Vitrio.Application.Orders.EraseOrderPersonalData;
using Vitrio.Application.Orders.ExpireOrders;
using Vitrio.Application.Orders.RecalculateProviderStats;

namespace Vitrio.Jobs.Orders;

public sealed class OrderJobs(ISender sender)
{
    private const int BatchSize = 200;

    public async Task ExpireAsync(CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new ExpireOrdersCommand(BatchSize), cancellationToken));

    public async Task CompleteForgottenAsync(CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new CompleteForgottenOrdersCommand(BatchSize), cancellationToken));

    public async Task ErasePersonalDataAsync(CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new EraseOrderPersonalDataCommand(BatchSize), cancellationToken));

    public async Task RecalculateProviderStatsAsync(CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new RecalculateProviderStatsCommand(), cancellationToken));
}
