using Vitrio.Application.Abstractions;
using Vitrio.Domain.Common;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class FakeCityDirectory : ICityDirectory
{
    public const int JaboticabalCode = 3524303;
    public const int SaoPauloCode = 3550308;

    public Task<Result<IReadOnlyList<IbgeCity>>> ListAsync(string state, CancellationToken cancellationToken)
    {
        IReadOnlyList<IbgeCity> cities = state == "SP"
            ? [new IbgeCity(JaboticabalCode, "Jaboticabal"), new IbgeCity(SaoPauloCode, "São Paulo")]
            : [];
        return Task.FromResult(Result<IReadOnlyList<IbgeCity>>.Success(cities));
    }
}
