using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderAddressResponse(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string CityName,
    string State,
    string Cep,
    double? Latitude,
    double? Longitude)
{
    public static OrderAddressResponse From(OrderAddress address, OrderPlace place) => new(
        address.Street,
        address.Number,
        address.Complement,
        place.Neighborhood,
        place.CityName,
        place.State,
        address.Cep,
        place.IsApproximate ? null : place.Latitude,
        place.IsApproximate ? null : place.Longitude);
}
