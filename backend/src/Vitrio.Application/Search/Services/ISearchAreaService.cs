using Vitrio.Application.Search.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Search.Services;

public interface ISearchAreaService
{
    Task<Result<SearchArea>> ResolveAsync(double? latitude, double? longitude, int? cityIbgeCode, CancellationToken cancellationToken);
}
