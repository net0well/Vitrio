using System.Text.Json.Serialization;

namespace Vitrio.Infrastructure.External.BrasilApi;

internal sealed record BrasilApiCnpjResponse(
    [property: JsonPropertyName("razao_social")] string? CompanyName,
    [property: JsonPropertyName("situacao_cadastral")] int? RegistrationStatus,
    [property: JsonPropertyName("opcao_pelo_mei")] bool? OptedForMei);
