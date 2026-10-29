using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ResendCode;

public sealed class ResendCodeCommandHandler(
    IEmailCodeService emailCodes,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<ResendCodeCommand, Result>
{
    public async ValueTask<Result> Handle(ResendCodeCommand command, CancellationToken cancellationToken)
    {
        var message = await emailCodes.PrepareAsync(EmailAddress.Create(command.Email).Value, command.Purpose, cancellationToken);
        if (message.IsFailure)
        {
            return message.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return message.Value is null ? Result.Success() : await emailSender.SendAsync(message.Value, cancellationToken);
    }
}
