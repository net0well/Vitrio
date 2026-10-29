using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Services;

public interface IAddressLookupService
{
    Task<Result<PostalLocation>> ResolveAsync(Cep cep, ManualAddress manual, CancellationToken cancellationToken);
}
