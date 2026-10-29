using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IAddressWritableRepository
{
    Task<Address?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Address address);

    void Remove(Address address);
}
