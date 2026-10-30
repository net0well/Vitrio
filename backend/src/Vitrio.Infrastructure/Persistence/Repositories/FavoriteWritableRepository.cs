using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Favorites.Repositories;
using Vitrio.Domain.Favorites;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class FavoriteWritableRepository(AppDbContext db) : IFavoriteWritableRepository
{
    public Task<Favorite?> GetForUserAsync(Guid userId, Guid providerId, CancellationToken cancellationToken) =>
        db.Favorites.SingleOrDefaultAsync(favorite => favorite.UserId == userId && favorite.ProviderId == providerId, cancellationToken);

    public Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Favorites.CountAsync(favorite => favorite.UserId == userId, cancellationToken);

    public void Add(Favorite favorite) => db.Favorites.Add(favorite);

    public void Remove(Favorite favorite) => db.Favorites.Remove(favorite);

    public async Task RemoveAllForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Favorites.RemoveRange(await db.Favorites.Where(favorite => favorite.UserId == userId).ToListAsync(cancellationToken));
}
