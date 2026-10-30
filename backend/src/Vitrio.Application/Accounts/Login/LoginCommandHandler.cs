using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Accounts.Login;

public sealed class LoginCommandHandler(
    IUserWritableRepository users,
    IPasswordService passwords,
    ISessionService sessions,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<LoginCommand, Result<AuthResponse>>
{
    public async ValueTask<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email).Value;
        var user = await users.GetByEmailAsync(email, cancellationToken);
        if (user is null || user.Status == UserStatus.Deleted)
        {
            passwords.MatchWithoutAccount(command.Password);
            return AccountErrors.InvalidCredentials;
        }

        var lockout = user.LockoutRemaining(clock);
        if (lockout > TimeSpan.Zero)
        {
            return AccountErrors.LockedOut(lockout);
        }

        var check = passwords.Check(user.PasswordHash, command.Password);
        if (check == PasswordCheck.Failed)
        {
            if (user.RegisterFailedSignIn(clock))
            {
                auditLog.Record(AuditActions.SignInLockedOut, null, AuditTargets.User, user.Id);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return AccountErrors.InvalidCredentials;
        }

        if (user.Status == UserStatus.PendingVerification)
        {
            return AccountErrors.EmailNotVerified;
        }

        if (user.Status == UserStatus.Suspended)
        {
            return AccountErrors.Suspended;
        }

        user.RegisterSuccessfulSignIn();
        if (check == PasswordCheck.SucceededRehashNeeded)
        {
            user.ReplacePasswordHash(passwords.Hash(command.Password));
        }

        var response = await sessions.StartAsync(user, command.DeviceName, cancellationToken);
        auditLog.Record(AuditActions.SignedIn, user.Id, AuditTargets.User, user.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}
