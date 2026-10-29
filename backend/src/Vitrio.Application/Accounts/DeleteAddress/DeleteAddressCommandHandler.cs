using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.DeleteAddress;

public sealed class DeleteAddressCommandHandler(ICurrentUser currentUser, IAddressWritableRepository addresses, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAddressCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteAddressCommand command, CancellationToken cancellationToken)
    {
        var address = await addresses.GetForUserAsync(command.Id, currentUser.Id, cancellationToken);
        if (address is null)
        {
            return AccountErrors.AddressNotFound;
        }

        addresses.Remove(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
