namespace Vitrio.Application.Search.Common;

public sealed record ProviderSearchPage(
    IReadOnlyList<ProviderSearchItem> Items,
    int Page,
    int PageSize,
    bool HasMore,
    SearchContextResponse Context);
