using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vitrio.Application.Favorites.Repositories;
using Vitrio.Application.Search.Common;
using Vitrio.Infrastructure.Persistence.Search;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class FavoriteSearchableRepository(
    AppDbContext db,
    ProviderListingReader reader,
    IOptions<SearchRelevanceOptions> relevance) : IFavoriteSearchableRepository
{
    public Task<IReadOnlyList<ProviderSearchItem>> ListAsync(Guid userId, int skip, int take, DateTimeOffset now, CancellationToken cancellationToken) =>
        reader.ItemsAsync(ProviderListingSql.Favorites(userId, skip, take, now, relevance.Value), cancellationToken);

    public Task<bool> IsFavoriteAsync(Guid userId, Guid providerId, CancellationToken cancellationToken) =>
        db.Favorites.AsNoTracking().AnyAsync(favorite => favorite.UserId == userId && favorite.ProviderId == providerId, cancellationToken);
}
