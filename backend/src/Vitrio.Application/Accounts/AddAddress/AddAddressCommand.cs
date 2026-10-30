using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.AddAddress;

public sealed record AddAddressCommand(
    string Label,
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
