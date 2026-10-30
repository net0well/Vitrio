using Mediator;
using Vitrio.Application.Favorites.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Favorites.ListFavorites;

public sealed record ListFavoritesQuery(int Page) : IQuery<Result<FavoritesPageResponse>>
{
    public const int PageSize = 20;
    public const int MaxPage = 10;
}
