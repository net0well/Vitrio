using Vitrio.Application.Search.Common;

namespace Vitrio.Application.Favorites.Common;

public sealed record FavoritesPageResponse(IReadOnlyList<ProviderSearchItem> Items, int Page, int PageSize, bool HasMore);
