using System.Net;
using Vitrio.Api.IntegrationTests.Infrastructure;

namespace Vitrio.Api.IntegrationTests.Health;

public sealed class HealthTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Live_ReturnsHealthyWithoutAuthentication()
    {
        var response = await _client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Ready_WhenDatabaseIsReachable_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
