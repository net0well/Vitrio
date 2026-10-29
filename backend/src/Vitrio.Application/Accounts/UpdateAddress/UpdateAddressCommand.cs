using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.UpdateAddress;

public sealed record UpdateAddressCommand(
    Guid Id, string Label,
    string Cep,
    string Number,
    string? Complement,
    string? Street,
    string? Neighborhood,
    string? State,
    int? CityIbgeCode) : ICommand<Result<AddressResponse>>
{
    public ManualAddress Manual => new(Street, Neighborhood, State, CityIbgeCode);
}
