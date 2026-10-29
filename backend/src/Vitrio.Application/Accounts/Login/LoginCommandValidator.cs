using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.Login;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).MustBeEmail();
        RuleFor(command => command.Password).NotEmpty().WithMessage("Digite sua senha.");
    }
}
