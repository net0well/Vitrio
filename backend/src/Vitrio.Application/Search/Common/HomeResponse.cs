using Vitrio.Application.Locations.Common;
using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Search.Common;

public sealed record HomeResponse(
    CitySummary? City,
    IReadOnlyList<CategoryResponse> Categories,
    IReadOnlyList<ProviderSearchItem> TopRated,
    bool HasProviders);
