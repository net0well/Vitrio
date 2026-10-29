using FluentValidation;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.RegisterDevice;

internal sealed class RegisterDeviceCommandValidator : AbstractValidator<RegisterDeviceCommand>
{
    public RegisterDeviceCommandValidator()
    {
        RuleFor(command => command.InstallationId).Must(Device.IsValidInstallationId).WithMessage("Identificador de instalação inválido.");
        RuleFor(command => command.Platform).IsInEnum();
        RuleFor(command => command.PushToken).NotEmpty().MaximumLength(Device.PushTokenMaxLength);
    }
}
