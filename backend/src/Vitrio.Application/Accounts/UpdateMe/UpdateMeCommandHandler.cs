using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.UpdateMe;

public sealed class UpdateMeCommandHandler(ICurrentUser currentUser, IUserWritableRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateMeCommand, Result<MeResponse>>
{
    public async ValueTask<Result<MeResponse>> Handle(UpdateMeCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(currentUser.Id, cancellationToken);
        if (user is null)
        {
            return AccountErrors.AccountNotFound;
        }

        if (command.DisplayName is not null)
        {
            user.Rename(DisplayName.Create(command.DisplayName).Value);
        }

        if (command.PhoneNumber is not null)
        {
            user.ChangePhoneNumber(command.PhoneNumber.Length == 0 ? null : PhoneNumber.Create(command.PhoneNumber).Value);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MeResponse.From(user, AccountRoles.Customer);
    }
}
