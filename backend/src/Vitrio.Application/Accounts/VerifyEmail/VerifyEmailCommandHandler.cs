using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.VerifyEmail;

public sealed class VerifyEmailCommandHandler(
    IUserWritableRepository users,
    IEmailCodeWritableRepository emailCodes,
    IEmailCodeService emailCodeService,
    IPasswordService passwords,
    ISessionService sessions,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<VerifyEmailCommand, Result<AuthResponse>>
{
    public async ValueTask<Result<AuthResponse>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email).Value;
        var user = await users.GetPendingByEmailAsync(email, cancellationToken);
        var emailCode = await emailCodes.GetLatestAsync(email, EmailCodePurpose.VerifyEmail, cancellationToken);
        if (user is null || emailCode is null)
        {
            passwords.MatchWithoutAccount(command.Password);
            return AccountErrors.InvalidCode;
        }

        if (!passwords.Matches(user.PasswordHash, command.Password))
        {
            emailCode.CountFailedAttempt();
            await TrySaveAsync(cancellationToken);
            return AccountErrors.InvalidCode;
        }

        if (!emailCodeService.Matches(emailCode, email, command.Code))
        {
            await TrySaveAsync(cancellationToken);
            return AccountErrors.InvalidCode;
        }

        user.ConfirmEmail(clock);
        var response = sessions.Start(user, command.DeviceName);
        auditLog.Record(AuditActions.EmailVerified, user.Id, AuditTargets.User, user.Id);

        return await TrySaveAsync(cancellationToken) ? response : AccountErrors.InvalidCode;
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
