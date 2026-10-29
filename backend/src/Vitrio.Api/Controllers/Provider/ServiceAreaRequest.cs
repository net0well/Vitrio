using Vitrio.Application.Providers.SetServiceArea;

namespace Vitrio.Api.Controllers.Provider;

public sealed record ServiceAreaRequest(string? Cep, string? Number, string? Complement, int? RadiusKm, IReadOnlyList<string>? Neighborhoods)
{
    public SetServiceAreaCommand ToCommand() =>
        new(Cep ?? string.Empty, Number ?? string.Empty, Complement, RadiusKm ?? 0, Neighborhoods ?? []);
}
