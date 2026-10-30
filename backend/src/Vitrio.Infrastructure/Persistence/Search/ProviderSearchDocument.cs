using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Search;

internal sealed record ProviderSearchDocument(string Primary, string Name, string Secondary, string Terms)
{
    public static ProviderSearchDocument From(Provider provider, IReadOnlyDictionary<int, string> categoryNames)
    {
        var categories = provider.Categories
            .OrderByDescending(category => category.IsPrimary)
            .Select(category => categoryNames.GetValueOrDefault(category.CategoryId));
        var services = provider.Services.OrderBy(service => service.SortOrder).ToList();
        var primary = SearchText.Normalize(Join([provider.Headline, .. categories, .. services.Select(service => service.Title)]));
        var name = SearchText.Normalize(provider.DisplayName);
        var secondary = SearchText.Normalize(Join([.. services.Select(service => service.Description), provider.About]));
        return new ProviderSearchDocument(primary, name, secondary, Join([primary, name]));
    }

    private static string Join(IEnumerable<string?> parts) => string.Join(' ', parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}
