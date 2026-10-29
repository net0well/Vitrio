using Mediator;
using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.VerifyEmail;

public sealed class VerifyEmailCommandHandler(
    IAppDbContext db,
    IVerificationCodes codes,
    IPasswordHasher passwordHasher,
    SessionStarter sessionStarter,
    IAuditLog auditLog,
    IClock clock) : ICommandHandler<VerifyEmailCommand, Result<AuthResponse>>
{
    public async ValueTask<Result<AuthResponse>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email).Value;
        var user = await db.Users.SingleOrDefaultAsync(
            account => account.Email == email.Value && account.Status == UserStatus.PendingVerification,
            cancellationToken);
        var emailCode = await db.EmailCodes
            .Where(code => code.Email == email.Value && code.Purpose == EmailCodePurpose.VerifyEmail)
            .OrderByDescending(code => code.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var password = PasswordPolicy.Normalize(command.Password);
        if (user is null || emailCode is null)
        {
            passwordHasher.Hash(password);
            return AccountErrors.InvalidCode;
        }

        if (passwordHasher.Verify(user.PasswordHash, password) == PasswordCheck.Failed)
        {
            emailCode.CountFailedAttempt();
            await SaveAsync(cancellationToken);
            return AccountErrors.InvalidCode;
        }

        var verification = emailCode.Verify(codes.Hash(email, EmailCodePurpose.VerifyEmail, command.Code), clock);
        if (verification.IsFailure)
        {
            await SaveAsync(cancellationToken);
            return verification.Error;
        }

        user.ConfirmEmail(clock);
        var response = sessionStarter.Start(user, command.DeviceName);
        auditLog.Record(AuditActions.EmailVerified, user.Id, AuditTargets.User, user.Id);

        return await SaveAsync(cancellationToken) ? response : AccountErrors.InvalidCode;
    }

    private async Task<bool> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}
