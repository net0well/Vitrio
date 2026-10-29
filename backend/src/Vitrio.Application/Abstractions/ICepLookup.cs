using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Abstractions;

public interface ICepLookup
{
    Task<Result<CepLookupResult?>> FindAsync(Cep cep, CancellationToken cancellationToken);
}
