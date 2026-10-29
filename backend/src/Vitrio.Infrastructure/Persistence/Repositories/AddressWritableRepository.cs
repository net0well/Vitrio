using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class AddressWritableRepository(AppDbContext db) : IAddressWritableRepository
{
    public Task<Address?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken) =>
        db.Addresses.SingleOrDefaultAsync(address => address.Id == id && address.UserId == userId, cancellationToken);

    public Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Addresses.CountAsync(address => address.UserId == userId, cancellationToken);

    public void Add(Address address) => db.Addresses.Add(address);

    public void Remove(Address address) => db.Addresses.Remove(address);
}
