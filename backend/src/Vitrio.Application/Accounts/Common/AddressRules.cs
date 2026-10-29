using FluentValidation;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Common;

internal static class AddressRules
{
    public static void ApplyAddressRules<T>(
        this AbstractValidator<T> validator,
        Func<T, string> label,
        Func<T, string> cep,
        Func<T, string> number,
        Func<T, string?> complement,
        Func<T, string?> state,
        Func<T, int?> cityIbgeCode)
    {
        validator.RuleFor(command => label(command))
            .NotEmpty().WithMessage("Dê um nome ao endereço, como Casa ou Trabalho.")
            .MaximumLength(Address.LabelMaxLength)
            .OverridePropertyName("Label");
        validator.RuleFor(command => cep(command))
            .Must(value => Cep.Create(value).IsSuccess).WithMessage(AccountErrors.InvalidCep.Message)
            .OverridePropertyName("Cep");
        validator.RuleFor(command => number(command))
            .NotEmpty().WithMessage("Informe o número.")
            .MaximumLength(Address.NumberMaxLength)
            .OverridePropertyName("Number");
        validator.RuleFor(command => complement(command))
            .MaximumLength(Address.ComplementMaxLength)
            .OverridePropertyName("Complement");
        validator.RuleFor(command => state(command))
            .Must(PostalLocation.IsState).When(command => state(command) is not null).WithMessage("Informe a UF com 2 letras.")
            .OverridePropertyName("State");
        validator.RuleFor(command => cityIbgeCode(command))
            .InclusiveBetween(1_000_000, 9_999_999).When(command => cityIbgeCode(command) is not null).WithMessage("Escolha a cidade na lista.")
            .OverridePropertyName("CityIbgeCode");
    }
}
