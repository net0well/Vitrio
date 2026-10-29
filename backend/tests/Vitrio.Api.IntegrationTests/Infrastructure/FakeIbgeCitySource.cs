using System.Collections.Concurrent;
using System.Globalization;
using Vitrio.Application.Abstractions;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class FakeIbgeCitySource : IIbgeCitySource
{
    public const int JaboticabalCode = 3524303;
    public const int SaoPauloCode = 3550308;
    public const int SantosCode = 3548500;
    public const int SaoJoseDosCamposCode = 3549904;
    public const int SaoJoseCode = 4216602;

    private static readonly IReadOnlyList<(int Code, string Name, string State, double West, double South, double East, double North)> Cities =
    [
        (JaboticabalCode, "Jaboticabal", "SP", -48.40, -21.32, -48.25, -21.18),
        (SaoPauloCode, "São Paulo", "SP", -46.83, -23.80, -46.36, -23.36),
        (SantosCode, "Santos", "SP", -46.40, -23.99, -46.28, -23.90),
        (SaoJoseDosCamposCode, "São José dos Campos", "SP", -46.00, -23.30, -45.70, -23.00),
        (SaoJoseCode, "São José", "SC", -48.70, -27.65, -48.60, -27.55)
    ];

    public ConcurrentDictionary<int, string> Renamed { get; } = new();

    public Task<IReadOnlyList<IbgeMunicipality>> ListStateAsync(string state, CancellationToken cancellationToken)
    {
        IReadOnlyList<IbgeMunicipality> found = Cities
            .Where(city => city.State == state)
            .Select(city => new IbgeMunicipality(
                city.Code,
                Renamed.TryGetValue(city.Code, out var name) ? name : city.Name,
                city.State,
                Rectangle(city.West, city.South, city.East, city.North)))
            .ToList();
        return Task.FromResult(found);
    }

    private static string Rectangle(double west, double south, double east, double north) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{{\"type\":\"Polygon\",\"coordinates\":[[[{west},{south}],[{east},{south}],[{east},{north}],[{west},{north}],[{west},{south}]]]}}");
}
