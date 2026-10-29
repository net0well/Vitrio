using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.ResendCode;

internal sealed class ResendCodeCommandValidator : AbstractValidator<ResendCodeCommand>
{
    public ResendCodeCommandValidator()
    {
        RuleFor(command => command.Email).MustBeEmail();
        RuleFor(command => command.Purpose).IsInEnum().WithMessage("Tipo de código inválido.");
    }
}
