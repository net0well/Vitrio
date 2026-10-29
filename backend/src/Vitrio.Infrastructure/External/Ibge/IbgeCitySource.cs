using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.External.Ibge;

internal sealed partial class IbgeCitySource(HttpClient http, ILogger<IbgeCitySource> logger) : IIbgeCitySource
{
    private const int StateCodeDivisor = 100_000;

    public async Task<IReadOnlyList<IbgeMunicipality>> ListStateAsync(string state, CancellationToken cancellationToken)
    {
        var names = await http.GetFromJsonAsync<List<IbgeLocalityRow>>(
            $"v1/localidades/estados/{state}/municipios?view=nivelado",
            cancellationToken) ?? [];
        var byCode = names.ToDictionary(row => row.Id, row => row.Name);
        var populations = byCode.Count == 0
            ? new Dictionary<int, int>()
            : await PopulationsAsync(byCode.Keys.First() / StateCodeDivisor, state, cancellationToken);

        using var boundaries = await JsonDocument.ParseAsync(
            await http.GetStreamAsync(
                $"v3/malhas/estados/{state}?formato=application/vnd.geo%2Bjson&qualidade=minima&intrarregiao=municipio",
                cancellationToken),
            cancellationToken: cancellationToken);

        var municipalities = new List<IbgeMunicipality>();
        foreach (var feature in boundaries.RootElement.GetProperty("features").EnumerateArray())
        {
            var code = int.Parse(feature.GetProperty("properties").GetProperty("codarea").GetString()!, CultureInfo.InvariantCulture);
            if (byCode.TryGetValue(code, out var name))
            {
                municipalities.Add(new IbgeMunicipality(
                    code,
                    name,
                    state,
                    populations.GetValueOrDefault(code),
                    feature.GetProperty("geometry").GetRawText()));
            }
        }

        if (municipalities.Count == 0)
        {
            throw new HttpRequestException($"IBGE returned no municipality boundaries for {state}.");
        }

        if (municipalities.Count < byCode.Count)
        {
            LogMissingBoundaries(logger, byCode.Count - municipalities.Count, state);
        }

        return municipalities;
    }

    private async Task<Dictionary<int, int>> PopulationsAsync(int stateCode, string state, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(
                await http.GetStreamAsync(
                    string.Create(CultureInfo.InvariantCulture, $"v3/agregados/6579/periodos/-1/variaveis/9324?localidades=N6[N3[{stateCode}]]"),
                    cancellationToken),
                cancellationToken: cancellationToken);

            var populations = new Dictionary<int, int>();
            foreach (var series in document.RootElement[0].GetProperty("resultados")[0].GetProperty("series").EnumerateArray())
            {
                var code = int.Parse(series.GetProperty("localidade").GetProperty("id").GetString()!, CultureInfo.InvariantCulture);
                var latest = series.GetProperty("serie").EnumerateObject().Last().Value.GetString();
                if (int.TryParse(latest, NumberStyles.None, CultureInfo.InvariantCulture, out var population))
                {
                    populations[code] = population;
                }
            }

            return populations;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or KeyNotFoundException or IndexOutOfRangeException or InvalidOperationException)
        {
            LogPopulationUnavailable(logger, state, exception.GetType().Name);
            return [];
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped {Count} municipalities of {State} that have no boundary in the IBGE map yet")]
    private static partial void LogMissingBoundaries(ILogger logger, int count, string state);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Population estimates of {State} are unavailable ({FailureType}); its cities are searched by name only")]
    private static partial void LogPopulationUnavailable(ILogger logger, string state, string failureType);
}
