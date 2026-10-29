using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.AddAddress;

public sealed class AddAddressCommandHandler(
    ICurrentUser currentUser,
    IAddressWritableRepository addresses,
    IAddressLookupService addressLookup,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<AddAddressCommand, Result<AddressResponse>>
{
    public async ValueTask<Result<AddressResponse>> Handle(AddAddressCommand command, CancellationToken cancellationToken)
    {
        if (await addresses.CountForUserAsync(currentUser.Id, cancellationToken) >= Address.MaxPerUser)
        {
            return AccountErrors.TooManyAddresses;
        }

        var cep = Cep.Create(command.Cep).Value;
        var location = await addressLookup.ResolveAsync(cep, command.Manual, cancellationToken);
        if (location.IsFailure)
        {
            return location.Error;
        }

        var address = Address.Create(currentUser.Id, command.Label, cep, location.Value, command.Number, command.Complement, clock);
        if (address.IsFailure)
        {
            return address.Error;
        }

        addresses.Add(address.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AddressResponse.From(address.Value);
    }
}
