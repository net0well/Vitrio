using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Common;

public sealed record AddressResponse(
    Guid Id,
    string Label,
    string Cep,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    int CityIbgeCode)
{
    public static AddressResponse From(Address address) => new(
        address.Id,
        address.Label,
        address.Cep,
        address.Street,
        address.Number,
        address.Complement,
        address.Neighborhood,
        address.City,
        address.State,
        address.CityIbgeCode);
}
