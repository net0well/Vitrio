using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Accounts.GrantAdmin;

public sealed class GrantAdminCommandHandler(
    IUserWritableRepository users,
    IAdminGrantWritableRepository adminGrants,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<GrantAdminCommand, Result>
{
    public async ValueTask<Result> Handle(GrantAdminCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(EmailAddress.Create(command.Email).Value, cancellationToken);
        if (user is not { Status: UserStatus.Active })
        {
            return AccountErrors.AccountNotFound;
        }

        if (await adminGrants.ExistsAsync(user.Id, cancellationToken))
        {
            return Result.Success();
        }

        adminGrants.Add(AdminGrant.For(user.Id, clock));
        auditLog.Record(AuditActions.AdminGranted, null, AuditTargets.User, user.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
