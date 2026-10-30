using System.Text.Json.Serialization;

namespace Vitrio.Application.Providers.Common;

public sealed record AssetLinkTarget(
    [property: JsonPropertyName("namespace")] string Namespace,
    [property: JsonPropertyName("package_name")] string PackageName,
    [property: JsonPropertyName("sha256_cert_fingerprints")] IReadOnlyList<string> Fingerprints);
