using Vitrio.Shared.Results;

namespace Vitrio.Application.Search.Common;

public static class SearchErrors
{
    public static readonly Error CityNotFound = Error.Validation("search.city_not_found", "Cidade não encontrada.");

    public static readonly Error NeedsLocation =
        Error.Validation("search.needs_location", "Ordenar por distância e limitar a distância só funcionam com a sua localização.");
}
