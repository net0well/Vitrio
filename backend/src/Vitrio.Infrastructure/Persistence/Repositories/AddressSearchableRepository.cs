using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class AddressSearchableRepository(AppDbContext db) : IAddressSearchableRepository
{
    public async Task<IReadOnlyList<AddressResponse>> ListForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Addresses
            .AsNoTracking()
            .Where(address => address.UserId == userId)
            .OrderBy(address => address.CreatedAt)
            .Select(address => new AddressResponse(
                address.Id,
                address.Label,
                address.Cep,
                address.Street,
                address.Number,
                address.Complement,
                address.Neighborhood,
                address.City,
                address.State,
                address.CityIbgeCode))
            .ToListAsync(cancellationToken);
}
