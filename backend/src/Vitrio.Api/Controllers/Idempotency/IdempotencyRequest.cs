using System.Security.Cryptography;
using System.Text.Json;

namespace Vitrio.Api.Controllers.Idempotency;

public static class IdempotencyRequest
{
    public const string Header = "Idempotency-Key";

    public static string Hash<TRequest>(TRequest request) =>
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(request, JsonSerializerOptions.Web)));
}
