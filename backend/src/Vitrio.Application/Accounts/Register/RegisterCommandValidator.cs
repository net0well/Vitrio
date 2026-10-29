using FluentValidation;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Register;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.DisplayName).MustBeDisplayName();
        RuleFor(command => command.Email).MustBeEmail();
        RuleFor(command => command.Password).MustBeStrongPassword(command => command.Email);
        RuleFor(command => command.AcceptedTermsVersion)
            .NotEmpty()
            .WithMessage("Aceite os termos de uso e a política de privacidade para continuar.")
            .MaximumLength(User.TermsVersionMaxLength);
    }
}
