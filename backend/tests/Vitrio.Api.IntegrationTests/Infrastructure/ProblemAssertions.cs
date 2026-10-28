using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

internal static class ProblemAssertions
{
    public static async Task<JsonElement> AssertProblemAsync(this HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal($"https://vitrio.app/problems/{code}", problem.GetProperty("type").GetString());
        Assert.Equal((int)status, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        return problem;
    }
}
