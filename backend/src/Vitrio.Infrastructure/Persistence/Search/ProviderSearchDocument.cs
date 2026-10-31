using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Search;

internal sealed record ProviderSearchDocument(
    string Primary,
    string Name,
    string Secondary,
    string Terms,
    double Completeness,
    long? ServicePriceCents)
{
    private const int PhotosForCompleteness = 3;
    private const double CompletenessItems = 4;

    public static ProviderSearchDocument From(Provider provider, IReadOnlyDictionary<int, string> categoryNames)
    {
        var categories = provider.Categories
            .OrderByDescending(category => category.IsPrimary)
            .Select(category => categoryNames.GetValueOrDefault(category.CategoryId));
        var services = provider.Services.OrderBy(service => service.SortOrder).ToList();
        var primary = SearchText.Normalize(Join([provider.Headline, .. categories, .. services.Select(service => service.Title)]));
        var name = SearchText.Normalize(provider.DisplayName);
        var secondary = SearchText.Normalize(Join([.. services.Select(service => service.Description), provider.About]));
        return new ProviderSearchDocument(
            primary,
            name,
            secondary,
            Join([primary, name]),
            CompletenessOf(provider),
            services
                .Where(service => !service.OnQuote && service.PriceUnit == PriceUnit.PerService)
                .Min(service => service.PriceFromCents));
    }

    private static double CompletenessOf(Provider provider)
    {
        bool[] items =
        [
            provider.AvatarKey is not null,
            provider.Photos.Count(photo => photo.Status == PhotoStatus.Active) >= PhotosForCompleteness,
            provider.Availability.Count > 0,
            provider.YearsOfExperience is not null
        ];
        return items.Count(present => present) / CompletenessItems;
    }

    private static string Join(IEnumerable<string?> parts) => string.Join(' ', parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}
