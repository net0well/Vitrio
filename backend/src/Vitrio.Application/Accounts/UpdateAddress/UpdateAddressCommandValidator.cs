using FluentValidation;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.UpdateAddress;

internal sealed class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
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
