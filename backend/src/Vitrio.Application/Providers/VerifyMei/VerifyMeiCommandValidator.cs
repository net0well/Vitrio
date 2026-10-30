using FluentValidation;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.VerifyMei;

internal sealed class VerifyMeiCommandValidator : AbstractValidator<VerifyMeiCommand>
{
    public VerifyMeiCommandValidator()
    {
        RuleFor(command => command.Cnpj)
            .Must(cnpj => Cnpj.Create(cnpj).IsSuccess).WithMessage(ProviderErrors.InvalidCnpj.Message);
    }
}
