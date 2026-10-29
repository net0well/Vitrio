using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.ResetPassword;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Email).MustBeEmail();
        RuleFor(command => command.Code).MustBeCode();
        RuleFor(command => command.NewPassword).MustBeStrongPassword(command => command.Email);
    }
}
