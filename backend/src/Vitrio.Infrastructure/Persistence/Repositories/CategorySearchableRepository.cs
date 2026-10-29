using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class CategorySearchableRepository(AppDbContext db) : ICategorySearchableRepository
{
    public async Task<IReadOnlyList<CategoryResponse>> ListActiveAsync(CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .Where(category => category.Active)
            .OrderBy(category => category.SortOrder)
            .Select(category => new CategoryResponse(category.Id, category.Slug, category.Name, category.Icon))
            .ToListAsync(cancellationToken);

    public Task<int> CountActiveAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
        db.Categories.AsNoTracking().CountAsync(category => ids.Contains(category.Id) && category.Active, cancellationToken);
}
