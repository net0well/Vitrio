using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Favorites.Common;
using Vitrio.Application.Favorites.Repositories;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Favorites.ListFavorites;

public sealed class ListFavoritesQueryHandler(
    ICurrentUser currentUser,
    IFavoriteSearchableRepository favorites,
    IClock clock) : IQueryHandler<ListFavoritesQuery, Result<FavoritesPageResponse>>
{
    public async ValueTask<Result<FavoritesPageResponse>> Handle(ListFavoritesQuery query, CancellationToken cancellationToken)
    {
        var items = await favorites.ListAsync(
            currentUser.Id,
            (query.Page - 1) * ListFavoritesQuery.PageSize,
            ListFavoritesQuery.PageSize + 1,
            clock.UtcNow,
            cancellationToken);
        return new FavoritesPageResponse(
            items.Take(ListFavoritesQuery.PageSize).ToList(),
            query.Page,
            ListFavoritesQuery.PageSize,
            items.Count > ListFavoritesQuery.PageSize);
    }
}
