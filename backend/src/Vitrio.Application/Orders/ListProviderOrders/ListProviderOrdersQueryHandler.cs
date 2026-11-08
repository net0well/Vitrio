using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.ListProviderOrders;

public sealed class ListProviderOrdersQueryHandler(IProviderOrderService providerOrders, IOrderSearchableRepository orders, IClock clock)
    : IQueryHandler<ListProviderOrdersQuery, Result<OrderProviderPage>>
{
    public async ValueTask<Result<OrderProviderPage>> Handle(ListProviderOrdersQuery query, CancellationToken cancellationToken)
    {
        if (await providerOrders.FindCurrentProviderIdAsync(cancellationToken) is not { } providerId)
        {
            return ProviderErrors.NotFound;
        }

        return await orders.ListForProviderAsync(
            providerId,
            query.Tab,
            clock.UtcNow,
            (query.Page - 1) * ListProviderOrdersQuery.PageSize,
            ListProviderOrdersQuery.PageSize,
            cancellationToken);
    }
}
