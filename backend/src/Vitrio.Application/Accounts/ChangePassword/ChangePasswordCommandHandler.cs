using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Accounts.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    ICurrentUser currentUser,
    IUserWritableRepository users,
    IPasswordService passwords,
    IPasswordChangeService passwordChanges,
    ISessionStatusCache sessionStatusCache,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ChangePasswordCommand, Result>
{
    public async ValueTask<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(currentUser.Id, cancellationToken);
        if (user is null)
        {
            return AccountErrors.AccountNotFound;
        }

        var lockout = user.LockoutRemaining(clock);
        if (lockout > TimeSpan.Zero)
        {
            return AccountErrors.LockedOut(lockout);
        }

        if (!passwords.Matches(user.PasswordHash, command.CurrentPassword))
        {
            if (user.RegisterFailedSignIn(clock))
            {
                auditLog.Record(AuditActions.SignInLockedOut, user.Id, AuditTargets.User, user.Id);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return AccountErrors.WrongCurrentPassword;
        }

        if (PasswordPolicy.FindProblem(command.NewPassword, user.Email) is { } problem)
        {
            return new ValidationError(new Dictionary<string, string[]>
            {
                [nameof(ChangePasswordCommand.NewPassword)] = [AccountRules.MessageFor(problem)]
            });
        }

        var revoked = await passwordChanges.ChangeAsync(
            user,
            command.NewPassword,
            SessionRevokedReasons.PasswordChanged,
            currentUser.SessionId,
            cancellationToken);
        auditLog.Record(AuditActions.PasswordChanged, user.Id, AuditTargets.User, user.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await sessionStatusCache.ForgetAsync(revoked, cancellationToken);
        await passwordChanges.NotifyAsync(user, cancellationToken);
        return Result.Success();
    }
}
