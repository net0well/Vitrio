using System.Text.Json.Serialization;

namespace Vitrio.Application.Providers.Common;

public sealed record AssetLinkResponse(
    [property: JsonPropertyName("relation")] IReadOnlyList<string> Relation,
    [property: JsonPropertyName("target")] AssetLinkTarget Target);
