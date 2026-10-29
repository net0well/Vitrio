using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ListAddresses;

public sealed record ListAddressesQuery : IQuery<Result<IReadOnlyList<AddressResponse>>>;
