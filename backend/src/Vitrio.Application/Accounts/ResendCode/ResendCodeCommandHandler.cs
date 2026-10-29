using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.ResendCode;

public sealed class ResendCodeCommandHandler(
    IUserSearchableRepository users,
    EmailCodeService emailCodes,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<ResendCodeCommand, Result>
{
    public async ValueTask<Result> Handle(ResendCodeCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email).Value;
        var code = await emailCodes.IssueAsync(email, command.Purpose, cancellationToken);
        if (code.IsFailure)
        {
            return code.Error;
        }

        var displayName = await users.FindPendingDisplayNameAsync(email, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (displayName is null)
        {
            return Result.Success();
        }

        var message = new VerificationCodeEmail(email.Value, DisplayName.FromStored(displayName).FirstName, code.Value);
        return await emailSender.SendAsync(message, cancellationToken);
    }
}
