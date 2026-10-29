using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.UpdateAddress;

public sealed class UpdateAddressCommandHandler(
    ICurrentUser currentUser,
    IAddressWritableRepository addresses,
    IAddressLookupService addressLookup,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<UpdateAddressCommand, Result<AddressResponse>>
{
    public async ValueTask<Result<AddressResponse>> Handle(UpdateAddressCommand command, CancellationToken cancellationToken)
    {
        var address = await addresses.GetForUserAsync(command.Id, currentUser.Id, cancellationToken);
        if (address is null)
        {
            return AccountErrors.AddressNotFound;
        }

        var cep = Cep.Create(command.Cep).Value;
        var location = await addressLookup.ResolveAsync(cep, command.Manual, cancellationToken);
        if (location.IsFailure)
        {
            return location.Error;
        }

        var update = address.Update(command.Label, cep, location.Value, command.Number, command.Complement, clock);
        if (update.IsFailure)
        {
            return update.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AddressResponse.From(address);
    }
}
