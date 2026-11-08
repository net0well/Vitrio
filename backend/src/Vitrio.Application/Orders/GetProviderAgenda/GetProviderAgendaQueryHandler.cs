using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.GetProviderAgenda;

public sealed class GetProviderAgendaQueryHandler(IProviderOrderService providerOrders, IOrderSearchableRepository orders, IClock clock)
    : IQueryHandler<GetProviderAgendaQuery, Result<IReadOnlyList<AgendaItemResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<AgendaItemResponse>>> Handle(GetProviderAgendaQuery query, CancellationToken cancellationToken)
    {
        if (await providerOrders.FindCurrentProviderIdAsync(cancellationToken) is not { } providerId)
        {
            return ProviderErrors.NotFound;
        }

        var items = await orders.ListAgendaAsync(providerId, query.From!.Value, query.To!.Value, clock.UtcNow, cancellationToken);
        return Result<IReadOnlyList<AgendaItemResponse>>.Success(items);
    }
}
