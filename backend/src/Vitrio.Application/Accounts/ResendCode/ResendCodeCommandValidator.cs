using FluentValidation;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.ResendCode;

internal sealed class ResendCodeCommandValidator : AbstractValidator<ResendCodeCommand>
{
    public ResendCodeCommandValidator()
    {
        RuleFor(command => command.Email).MustBeEmail();
        RuleFor(command => command.Purpose)
            .Equal(EmailCodePurpose.VerifyEmail)
            .WithMessage("Tipo de código não disponível.");
    }
}
