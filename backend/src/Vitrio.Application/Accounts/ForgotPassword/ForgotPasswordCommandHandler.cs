using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    IEmailCodeService emailCodes,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<ForgotPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email).Value;
        var message = await emailCodes.PrepareAsync(email, EmailCodePurpose.ResetPassword, cancellationToken);
        if (message.IsFailure)
        {
            return message.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return message.Value is null ? Result.Success() : await emailSender.SendAsync(message.Value, cancellationToken);
    }
}
