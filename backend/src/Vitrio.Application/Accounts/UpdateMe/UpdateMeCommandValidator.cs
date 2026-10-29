using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.UpdateMe;

internal sealed class UpdateMeCommandValidator : AbstractValidator<UpdateMeCommand>
{
    public UpdateMeCommandValidator()
    {
        RuleFor(command => command.DisplayName!).MustBeDisplayName().When(command => command.DisplayName is not null);
        RuleFor(command => command.PhoneNumber!).MustBePhoneNumber().When(command => !string.IsNullOrEmpty(command.PhoneNumber));
    }
}
