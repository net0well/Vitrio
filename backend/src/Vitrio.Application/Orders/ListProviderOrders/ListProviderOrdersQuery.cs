using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.ListProviderOrders;

public sealed record ListProviderOrdersQuery(ProviderOrderTab Tab, int Page) : IQuery<Result<OrderProviderPage>>
{
    public const int PageSize = 20;
    public const int MaxPage = 50;
}
