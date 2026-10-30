using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Services;

public interface IMeiCheckService
{
    Task<MeiCheck> CheckAsync(Cnpj cnpj, CancellationToken cancellationToken);
}
