using FluentValidation;

namespace Vitrio.Application.Favorites.ListFavorites;

internal sealed class ListFavoritesQueryValidator : AbstractValidator<ListFavoritesQuery>
{
    public ListFavoritesQueryValidator() => RuleFor(query => query.Page).InclusiveBetween(1, ListFavoritesQuery.MaxPage);
}
