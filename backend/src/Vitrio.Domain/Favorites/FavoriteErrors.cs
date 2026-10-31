using Vitrio.Shared.Results;

namespace Vitrio.Domain.Favorites;

public static class FavoriteErrors
{
    public static readonly Error LimitReached =
        Error.Conflict("favorite.limit_reached", "Você chegou a 200 favoritos. Remova algum para salvar outro.");
}
