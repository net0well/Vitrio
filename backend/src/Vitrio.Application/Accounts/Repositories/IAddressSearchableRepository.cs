using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.Repositories;

public interface IAddressSearchableRepository
{
    Task<IReadOnlyList<AddressResponse>> ListForUserAsync(Guid userId, CancellationToken cancellationToken);
}
