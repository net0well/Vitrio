using Vitrio.Application.Accounts.AddAddress;
using Vitrio.Application.Accounts.UpdateAddress;

namespace Vitrio.Api.Controllers.Me;

public sealed record AddressRequest(
    string? Label,
    string? Cep,
    string? Number,
    string? Complement,
    string? Street,
    string? Neighborhood,
    string? State,
    int? CityIbgeCode)
{
    public AddAddressCommand ToAddCommand() => new(
        Label ?? string.Empty,
        Cep ?? string.Empty,
        Number ?? string.Empty,
        Complement,
        Street,
        Neighborhood,
        State,
        CityIbgeCode);

    public UpdateAddressCommand ToUpdateCommand(Guid id) => new(
        id,
        Label ?? string.Empty,
        Cep ?? string.Empty,
        Number ?? string.Empty,
        Complement,
        Street,
        Neighborhood,
        State,
        CityIbgeCode);
}
