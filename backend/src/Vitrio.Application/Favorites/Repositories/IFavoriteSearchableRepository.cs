using Vitrio.Application.Search.Common;

namespace Vitrio.Application.Favorites.Repositories;

public interface IFavoriteSearchableRepository
{
    Task<IReadOnlyList<ProviderSearchItem>> ListAsync(Guid userId, int skip, int take, DateTimeOffset now, CancellationToken cancellationToken);

    Task<bool> IsFavoriteAsync(Guid userId, Guid providerId, CancellationToken cancellationToken);
}
