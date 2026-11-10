using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Search.Common;
using Vitrio.Application.Search.Repositories;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.GetOrderSuggestions;

public sealed class GetOrderSuggestionsQueryHandler(
    ICurrentUser currentUser,
    IOrderSearchableRepository orders,
    IProviderListingSearchableRepository listings,
    IClock clock) : IQueryHandler<GetOrderSuggestionsQuery, Result<IReadOnlyList<ProviderSearchItem>>>
{
    private const int MaxSuggestions = 3;

    public async ValueTask<Result<IReadOnlyList<ProviderSearchItem>>> Handle(GetOrderSuggestionsQuery query, CancellationToken cancellationToken)
    {
        var order = await orders.FindSuggestionContextAsync(query.OrderId, currentUser.Id, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        if (!order.AllowsSuggestions)
        {
            return OrderErrors.NoSuggestions;
        }

        var criteria = new ProviderListingCriteria(
            new SearchArea(SearchMode.NearMe, new Coordinates(order.Latitude, order.Longitude), null),
            null,
            order.CategoryId,
            SearchDistances.DefaultMaxMeters,
            null,
            false,
            false,
            null,
            SearchSort.Relevance,
            0,
            MaxSuggestions,
            clock.UtcNow)
        {
            ExcludedProviderId = order.ProviderId,
            ExcludedUserId = currentUser.Id,
        };
        return Result<IReadOnlyList<ProviderSearchItem>>.Success(await listings.SearchAsync(criteria, cancellationToken));
    }
}
