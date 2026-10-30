using Vitrio.Domain.Favorites;

namespace Vitrio.Application.Favorites.Repositories;

public interface IFavoriteWritableRepository
{
    Task<Favorite?> GetForUserAsync(Guid userId, Guid providerId, CancellationToken cancellationToken);

    Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Favorite favorite);

    void Remove(Favorite favorite);

    Task RemoveAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
