using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.CurrentPassword).NotEmpty().WithMessage("Digite sua senha atual.");
        RuleFor(command => command.NewPassword).MustBeStrongPassword(_ => null);
    }
}
