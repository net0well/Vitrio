using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Vitrio.Domain.Categories;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Search;

internal sealed class ProviderSearchIndexInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context && ChangedProviders(context) is { Count: > 0 } changed)
        {
            Index(changed, context.Set<Category>().AsNoTracking().ToDictionary(category => category.Id, category => category.Name));
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context && ChangedProviders(context) is { Count: > 0 } changed)
        {
            Index(
                changed,
                await context.Set<Category>().AsNoTracking().ToDictionaryAsync(category => category.Id, category => category.Name, cancellationToken));
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static List<EntityEntry<Provider>> ChangedProviders(DbContext context)
    {
        context.ChangeTracker.DetectChanges();
        return context.ChangeTracker.Entries<Provider>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .ToList();
    }

    private static void Index(IEnumerable<EntityEntry<Provider>> entries, IReadOnlyDictionary<int, string> categoryNames)
    {
        foreach (var entry in entries)
        {
            var document = ProviderSearchDocument.From(entry.Entity, categoryNames);
            entry.Property(ProviderSearchIndex.Primary).CurrentValue = document.Primary;
            entry.Property(ProviderSearchIndex.Name).CurrentValue = document.Name;
            entry.Property(ProviderSearchIndex.Secondary).CurrentValue = document.Secondary;
            entry.Property(ProviderSearchIndex.Terms).CurrentValue = document.Terms;
            entry.Property(ProviderSearchIndex.Completeness).CurrentValue = document.Completeness;
            entry.Property(ProviderSearchIndex.ServicePriceCents).CurrentValue = document.ServicePriceCents;
        }
    }
}
