using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.ListAddresses;

public sealed record ListAddressesQuery : IQuery<Result<IReadOnlyList<AddressResponse>>>;
