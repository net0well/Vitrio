using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static async Task<T> ReadApiAsync<T>(this HttpContent content) =>
        (await content.ReadFromJsonAsync<T>(Options, TestContext.Current.CancellationToken))!;
}
