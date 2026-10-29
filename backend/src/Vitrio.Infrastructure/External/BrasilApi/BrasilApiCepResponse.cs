namespace Vitrio.Infrastructure.External.BrasilApi;

internal sealed record BrasilApiCepResponse(
    string? Cep,
    string? State,
    string? City,
    string? Neighborhood,
    string? Street,
    BrasilApiLocation? Location);
