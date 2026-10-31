using Vitrio.Application.Search.Common;

namespace Vitrio.Application.Search.Services;

public interface ITopRatedProvidersService
{
    Task<IReadOnlyList<ProviderSearchItem>> ListAsync(SearchArea area, CancellationToken cancellationToken);
}
