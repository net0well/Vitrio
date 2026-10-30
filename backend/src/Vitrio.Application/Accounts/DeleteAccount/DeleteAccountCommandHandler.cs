using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Application.Favorites.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Accounts.DeleteAccount;

public sealed class DeleteAccountCommandHandler(
    ICurrentUser currentUser,
    IUserWritableRepository users,
    ISessionWritableRepository sessions,
    IAddressWritableRepository addresses,
    IDeviceWritableRepository devices,
    IFavoriteWritableRepository favorites,
    IEnumerable<IAccountDeletionBlocker> blockers,
    ISessionService sessionService,
    ISessionStatusCache sessionStatusCache,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<DeleteAccountCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(currentUser.Id, cancellationToken);
        if (user is null)
        {
            return AccountErrors.AccountNotFound;
        }

        foreach (var blocker in blockers)
        {
            if (await blocker.FindBlockerAsync(user.Id, cancellationToken) is { } reason)
            {
                return reason;
            }
        }

        var deletion = user.Delete(clock);
        if (deletion.IsFailure)
        {
            return deletion.Error;
        }

        var revoked = await sessionService.RevokeAsync(
            await sessions.ListUnrevokedForUserAsync(user.Id, cancellationToken),
            SessionRevokedReasons.AccountDeleted,
            cancellationToken);
        await addresses.RemoveAllForUserAsync(user.Id, cancellationToken);
        await devices.RemoveAllForUserAsync(user.Id, cancellationToken);
        await favorites.RemoveAllForUserAsync(user.Id, cancellationToken);
        auditLog.Record(AuditActions.AccountDeleted, user.Id, AuditTargets.User, user.Id);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await sessionStatusCache.ForgetAsync(revoked, cancellationToken);
        return Result.Success();
    }
}
