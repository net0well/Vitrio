using Vitrio.Shared.Results;

namespace Vitrio.Domain.Locations;

public static class LocationErrors
{
    public static readonly Error CityNotFound = Error.NotFound("city.not_found", "Cidade não encontrada.");

    public static readonly Error NoCityNearby =
        Error.NotFound("city.not_nearby", "Não encontramos uma cidade perto deste ponto. Escolha a cidade na lista.");

    public static readonly Error CepNotFound = Error.NotFound("cep.not_found", "Não encontramos este CEP.");
}
