using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.AddAddress;

internal sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        this.ApplyAddressRules(
            command => command.Label,
            command => command.Cep,
            command => command.Number,
            command => command.Complement,
            command => command.State,
            command => command.CityIbgeCode);
    }
}
