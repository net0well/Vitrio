using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Vitrio.Api.IntegrationTests.Infrastructure;

namespace Vitrio.Api.IntegrationTests.Documentation;

public sealed class SwaggerTests(ApiFactory factory)
{
    [Fact]
    public async Task SwaggerDocument_InDevelopment_DescribesVersionOneAndTheBearerScheme()
    {
        using var development = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        var client = development.CreateClient();

        var document = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        Assert.StartsWith("3.", document.GetProperty("openapi").GetString(), StringComparison.Ordinal);
        Assert.Equal("Vitrio API", document.GetProperty("info").GetProperty("title").GetString());
        var paths = document.GetProperty("paths");
        Assert.False(paths.GetProperty("/api/v1/probe/value").GetProperty("get").TryGetProperty("security", out _));
        Assert.True(paths.GetProperty("/api/v1/probe/private").GetProperty("get").TryGetProperty("security", out _));
        Assert.True(document.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));
    }

    [Fact]
    public async Task SwaggerUi_InDevelopment_IsServed()
    {
        using var development = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));

        var response = await development.CreateClient().GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerDocument_OutsideDevelopment_IsNotServed()
    {
        var response = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
