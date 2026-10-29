using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.GrantAdmin;

internal sealed class GrantAdminCommandValidator : AbstractValidator<GrantAdminCommand>
{
    public GrantAdminCommandValidator()
    {
        RuleFor(command => command.Email).MustBeEmail();
    }
}
