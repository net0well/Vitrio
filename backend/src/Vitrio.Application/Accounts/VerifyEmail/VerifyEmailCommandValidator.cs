using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.VerifyEmail;

internal sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(command => command.Email).MustBeEmail();
        RuleFor(command => command.Code).MustBeCode();
        RuleFor(command => command.Password).NotEmpty().WithMessage("Digite sua senha.");
    }
}
