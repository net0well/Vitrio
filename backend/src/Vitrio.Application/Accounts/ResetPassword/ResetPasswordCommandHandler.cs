using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Exceptions;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IUserWritableRepository users,
    IEmailCodeWritableRepository emailCodes,
    IEmailCodeService emailCodeService,
    IPasswordChangeService passwordChanges,
    ISessionService sessions,
    ISessionStatusCache sessionStatusCache,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork) : ICommandHandler<ResetPasswordCommand, Result<AuthResponse>>
{
    public async ValueTask<Result<AuthResponse>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email).Value;
        var user = await users.GetByEmailAsync(email, cancellationToken);
        var emailCode = await emailCodes.GetLatestAsync(email, EmailCodePurpose.ResetPassword, cancellationToken);
        if (user is not { Status: UserStatus.Active } || emailCode is null)
        {
            return AccountErrors.InvalidCode;
        }

        if (!emailCodeService.Matches(emailCode, email, command.Code))
        {
            await TrySaveAsync(cancellationToken);
            return AccountErrors.InvalidCode;
        }

        var revoked = await passwordChanges.ChangeAsync(
            user,
            command.NewPassword,
            SessionRevokedReasons.PasswordReset,
            sessionToKeep: null,
            cancellationToken);
        var response = await sessions.StartAsync(user, command.DeviceName, cancellationToken);
        auditLog.Record(AuditActions.PasswordReset, user.Id, AuditTargets.User, user.Id);
        if (!await TrySaveAsync(cancellationToken))
        {
            return AccountErrors.InvalidCode;
        }

        await sessionStatusCache.ForgetAsync(revoked, cancellationToken);
        await passwordChanges.NotifyAsync(user, cancellationToken);
        return response;
    }

    private async Task<bool> TrySaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (ConcurrencyConflictException)
        {
            return false;
        }
    }
}
