using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Abstractions;

public interface ICnpjLookup
{
    Task<Result<CnpjLookupResult?>> FindAsync(Cnpj cnpj, CancellationToken cancellationToken);
}
