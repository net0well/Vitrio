using System.Text.Json.Serialization;

namespace Vitrio.Infrastructure.External.Ibge;

internal sealed record IbgeLocalityRow(
    [property: JsonPropertyName("municipio-id")] int Id,
    [property: JsonPropertyName("municipio-nome")] string Name);
