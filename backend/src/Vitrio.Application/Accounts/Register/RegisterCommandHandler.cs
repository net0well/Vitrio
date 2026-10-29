using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Register;

public sealed class RegisterCommandHandler(
    IUserWritableRepository users,
    PasswordService passwords,
    EmailCodeService emailCodes,
    IEmailSender emailSender,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RegisterCommand, Result>
{
    public async ValueTask<Result> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email).Value;
        var displayName = DisplayName.Create(command.DisplayName).Value;
        var passwordHash = passwords.Hash(command.Password);

        var code = await emailCodes.IssueAsync(email, EmailCodePurpose.VerifyEmail, cancellationToken);
        if (code.IsFailure)
        {
            return code.Error;
        }

        var user = await users.GetByEmailAsync(email, cancellationToken);
        AccountEmail? message = null;
        if (user is null)
        {
            user = User.Register(email, displayName, passwordHash, command.AcceptedTermsVersion, clock);
            users.Add(user);
            auditLog.Record(AuditActions.AccountRegistered, user.Id, AuditTargets.User, user.Id);
            message = new VerificationCodeEmail(user.Email, displayName.FirstName, code.Value);
        }
        else if (user.Status == UserStatus.PendingVerification)
        {
            user.UpdatePendingRegistration(displayName, passwordHash, command.AcceptedTermsVersion, clock);
            message = new VerificationCodeEmail(user.Email, displayName.FirstName, code.Value);
        }
        else if (user.Status is (UserStatus.Active or UserStatus.Suspended) && user.TryMarkRegistrationAttemptNotified(clock))
        {
            message = new RegistrationAttemptEmail(user.Email, DisplayName.FromStored(user.DisplayName).FirstName);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return message is null ? Result.Success() : await emailSender.SendAsync(message, cancellationToken);
    }
}
