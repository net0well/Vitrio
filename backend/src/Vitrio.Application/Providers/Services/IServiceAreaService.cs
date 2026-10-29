using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.Services;

public interface IServiceAreaService
{
    Task<Result<ResolvedServiceArea>> ResolveAsync(Cep cep, string number, string? complement, CancellationToken cancellationToken);
}
