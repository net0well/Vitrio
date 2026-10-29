using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.ForgotPassword;

internal sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(command => command.Email).MustBeEmail();
    }
}
