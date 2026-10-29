using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class ProviderWritableRepository(AppDbContext db) : IProviderWritableRepository
{
    public Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Providers
            .Include(provider => provider.Categories)
            .Include(provider => provider.Services)
            .Include(provider => provider.Neighborhoods)
            .Include(provider => provider.Availability)
            .AsSplitQuery()
            .SingleOrDefaultAsync(provider => provider.UserId == userId && provider.Status != ProviderStatus.Removed, cancellationToken);

    public void Add(Provider provider) => db.Providers.Add(provider);
}
