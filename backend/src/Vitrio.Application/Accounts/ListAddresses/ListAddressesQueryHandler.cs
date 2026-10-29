using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.ListAddresses;

public sealed class ListAddressesQueryHandler(ICurrentUser currentUser, IAddressSearchableRepository addresses)
    : IQueryHandler<ListAddressesQuery, Result<IReadOnlyList<AddressResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<AddressResponse>>> Handle(ListAddressesQuery query, CancellationToken cancellationToken) =>
        Result<IReadOnlyList<AddressResponse>>.Success(await addresses.ListForUserAsync(currentUser.Id, cancellationToken));
}
