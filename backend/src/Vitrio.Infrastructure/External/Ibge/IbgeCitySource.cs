using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.External.Ibge;

internal sealed class IbgeCitySource(HttpClient http) : IIbgeCitySource
{
    public async Task<IReadOnlyList<IbgeMunicipality>> ListStateAsync(string state, CancellationToken cancellationToken)
    {
        var names = await http.GetFromJsonAsync<List<IbgeLocalityRow>>(
            $"v1/localidades/estados/{state}/municipios?view=nivelado",
            cancellationToken) ?? [];
        var byCode = names.ToDictionary(row => row.Id, row => row.Name);

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
                municipalities.Add(new IbgeMunicipality(code, name, state, feature.GetProperty("geometry").GetRawText()));
            }
        }

        if (municipalities.Count == 0 || municipalities.Count != byCode.Count)
        {
            throw new HttpRequestException(
                $"IBGE returned {byCode.Count} municipalities and {municipalities.Count} matching boundaries for {state}.");
        }

        return municipalities;
    }
}
