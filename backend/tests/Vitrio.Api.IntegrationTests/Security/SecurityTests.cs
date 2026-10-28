using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vitrio.Api.IntegrationTests.Infrastructure;

namespace Vitrio.Api.IntegrationTests.Security;

public sealed class SecurityTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorizedProblem()
    {
        var response = await _client.GetAsync("/api/v1/probe/private", TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "unauthorized");
        Assert.Contains(response.Headers.WwwAuthenticate, header => header.Scheme == "Bearer");
    }

    [Fact]
    public async Task ProtectedEndpoint_WithUnsignedToken_ReturnsUnauthorized()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/probe/private");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", UnsignedToken("""{"sub":"1","roles":"admin"}"""));

        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "unauthorized");
    }

    private static string UnsignedToken(string payload) =>
        $"{Base64UrlEncoder.Encode("""{"alg":"none","typ":"JWT"}""")}.{Base64UrlEncoder.Encode(payload)}.";

    [Fact]
    public async Task UnknownRoute_WithoutToken_ReturnsUnauthorizedWithoutRevealingTheRoute()
    {
        var response = await _client.GetAsync("/api/v1/does-not-exist", TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "unauthorized");
    }

    [Fact]
    public async Task UnhandledException_ReturnsProblemWithoutInternalDetails()
    {
        var response = await _client.GetAsync("/api/v1/probe/crash", TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.InternalServerError, "internal_error");
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("segredo", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(nameof(InvalidOperationException), body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Response_IncludesSecurityHeaders()
    {
        var response = await _client.GetAsync("/api/v1/probe/value", TestContext.Current.CancellationToken);

        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("DENY", Assert.Single(response.Headers.GetValues("X-Frame-Options")));
        Assert.Equal("no-referrer", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
        Assert.Equal(
            "default-src 'none'; frame-ancestors 'none'",
            Assert.Single(response.Headers.GetValues("Content-Security-Policy")));
    }

    [Fact]
    public async Task AnonymousResponse_IsNotCachedByDefault()
    {
        var response = await _client.GetAsync("/api/v1/probe/value", TestContext.Current.CancellationToken);

        Assert.True(response.Headers.CacheControl?.NoStore);
    }

    [Fact]
    public async Task PublicReferenceData_KeepsItsExplicitCachePolicy()
    {
        var response = await _client.GetAsync("/api/v1/probe/public-reference", TestContext.Current.CancellationToken);

        Assert.True(response.Headers.CacheControl?.Public);
        Assert.Equal(TimeSpan.FromHours(1), response.Headers.CacheControl?.MaxAge);
    }

    [Theory]
    [InlineData("/api/v1/probe/value")]
    [InlineData("/api/v1/probe/public-reference")]
    public async Task RequestWithAuthorizationHeader_IsNeverCached(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid");

        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.False(response.Headers.CacheControl?.Public);
    }

    [Fact]
    public async Task Request_WithUnknownJsonProperty_ReturnsValidationProblem()
    {
        using var content = new StringContent("""{"name":"Ana","role":"admin"}""", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/probe/echo", content, TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
    }

    [Fact]
    public void Kestrel_LimitsRequestBodyTo64Kilobytes()
    {
        var kestrel = factory.Services.GetRequiredService<IOptions<KestrelServerOptions>>().Value;

        Assert.Equal(64 * 1024, kestrel.Limits.MaxRequestBodySize);
    }

    [Fact]
    public async Task RateLimitedEndpoint_WhenLimitIsExceeded_ReturnsTooManyRequestsWithRetryAfter()
    {
        await _client.GetAsync("/api/v1/probe/limited", TestContext.Current.CancellationToken);

        var response = await _client.GetAsync("/api/v1/probe/limited", TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.TooManyRequests, "rate_limited");
        Assert.NotNull(response.Headers.RetryAfter);
    }
}
