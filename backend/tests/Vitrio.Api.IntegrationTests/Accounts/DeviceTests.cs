using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.IntegrationTests.Accounts;

public sealed class DeviceTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task RegisterDevice_StoresThePushTokenAndUpdatesItLater()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        var installation = NewInstallationId();

        var first = await _client.SendJsonAsAsync(HttpMethod.Put, $"/api/v1/me/devices/{installation}", auth.AccessToken, new { platform = "android", pushToken = "token-1" });
        var second = await _client.SendJsonAsAsync(HttpMethod.Put, $"/api/v1/me/devices/{installation}", auth.AccessToken, new { platform = "android", pushToken = "token-2" });

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        var device = await FindAsync(installation);
        Assert.Equal(auth.User.Id, device!.UserId);
        Assert.Equal("token-2", device.PushToken);
        Assert.Equal(DevicePlatform.Android, device.Platform);
    }

    [Fact]
    public async Task RegisterDevice_WhenAnotherAccountSignsInOnTheSamePhone_MovesTheDevice()
    {
        var first = await factory.SignInNewAccountAsync(_client);
        var second = await factory.SignInNewAccountAsync(_client);
        var installation = NewInstallationId();
        await _client.SendJsonAsAsync(HttpMethod.Put, $"/api/v1/me/devices/{installation}", first.AccessToken, new { platform = "android", pushToken = "token-1" });

        await _client.SendJsonAsAsync(HttpMethod.Put, $"/api/v1/me/devices/{installation}", second.AccessToken, new { platform = "android", pushToken = "token-1" });

        Assert.Equal(second.User.Id, (await FindAsync(installation))!.UserId);
    }

    [Fact]
    public async Task RegisterDevice_WithInvalidData_ReturnsValidationError()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Put, "/api/v1/me/devices/curto", auth.AccessToken, new { platform = "android", pushToken = "" });

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.True(problem.GetProperty("errors").TryGetProperty("installationId", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("pushToken", out _));
    }

    [Fact]
    public async Task RemoveDevice_OfAnotherPerson_KeepsItRegistered()
    {
        var owner = await factory.SignInNewAccountAsync(_client);
        var stranger = await factory.SignInNewAccountAsync(_client);
        var installation = NewInstallationId();
        await _client.SendJsonAsAsync(HttpMethod.Put, $"/api/v1/me/devices/{installation}", owner.AccessToken, new { platform = "ios", pushToken = "token-1" });

        var byStranger = await _client.SendAsAsync(HttpMethod.Delete, $"/api/v1/me/devices/{installation}", stranger.AccessToken);
        Assert.Equal(HttpStatusCode.NoContent, byStranger.StatusCode);
        Assert.NotNull(await FindAsync(installation));

        var byOwner = await _client.SendAsAsync(HttpMethod.Delete, $"/api/v1/me/devices/{installation}", owner.AccessToken);
        Assert.Equal(HttpStatusCode.NoContent, byOwner.StatusCode);
        Assert.Null(await FindAsync(installation));
    }

    [Theory]
    [InlineData("GET", "/api/v1/me")]
    [InlineData("PATCH", "/api/v1/me")]
    [InlineData("GET", "/api/v1/me/addresses")]
    [InlineData("POST", "/api/v1/me/addresses")]
    [InlineData("PUT", "/api/v1/me/addresses/0199a0c4-3f2e-7c1a-9d7e-2a4b6c8d0e1f")]
    [InlineData("DELETE", "/api/v1/me/addresses/0199a0c4-3f2e-7c1a-9d7e-2a4b6c8d0e1f")]
    [InlineData("PUT", "/api/v1/me/devices/instalacao-123")]
    [InlineData("DELETE", "/api/v1/me/devices/instalacao-123")]
    public async Task ProfileEndpoints_WithoutToken_ReturnUnauthorized(string method, string url)
    {
        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url), TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "unauthorized");
    }

    private Task<Device?> FindAsync(string installationId) =>
        factory.QueryAsync(db => db.Devices.AsNoTracking().SingleOrDefaultAsync(device => device.InstallationId == installationId, TestContext.Current.CancellationToken));

    private static string NewInstallationId() => Guid.NewGuid().ToString();
}
