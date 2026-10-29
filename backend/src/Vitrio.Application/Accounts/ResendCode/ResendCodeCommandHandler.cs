using Mediator;
using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.ResendCode;

public sealed class ResendCodeCommandHandler(IAppDbContext db, EmailCodeIssuer codeIssuer, IEmailSender emailSender)
    : ICommandHandler<ResendCodeCommand, Result>
{
    public async ValueTask<Result> Handle(ResendCodeCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email).Value;
        var code = await codeIssuer.IssueAsync(email, command.Purpose, cancellationToken);
        if (code.IsFailure)
        {
            return code.Error;
        }

        var displayName = await db.Users
            .AsNoTracking()
            .Where(account => account.Email == email.Value && account.Status == UserStatus.PendingVerification)
            .Select(account => account.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        if (displayName is null)
        {
            return Result.Success();
        }

        var message = new VerificationCodeEmail(email.Value, DisplayName.FromStored(displayName).FirstName, code.Value);
        return await emailSender.SendAsync(message, cancellationToken);
    }
}
