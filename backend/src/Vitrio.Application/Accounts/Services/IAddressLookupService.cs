using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.Services;

public interface IAddressLookupService
{
    Task<Result<CepDetails?>> LookupAsync(Cep cep, CancellationToken cancellationToken);

    Task<Result<PostalLocation>> ResolveAsync(Cep cep, ManualAddress manual, CancellationToken cancellationToken);
}
