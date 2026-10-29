using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.ListSessions;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.IntegrationTests.Accounts;

public sealed class SignInTests(ApiFactory factory)
{
    private const string ProtectedProbe = "/api/v1/probe/private";

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Login_WithTheRightPassword_OpensASessionForTheDevice()
    {
        var email = AuthApi.NewEmail();
        var user = await factory.SeedActiveUserAsync(email);

        var response = await _client.LoginAsync(email, deviceName: "Moto G84");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.ReadAuthAsync();
        Assert.Equal(user.Id, auth.User.Id);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsAsync(HttpMethod.Get, ProtectedProbe, auth.AccessToken)).StatusCode);
        var session = await factory.QueryAsync(db => db.Sessions.AsNoTracking().SingleAsync(s => s.UserId == user.Id, TestContext.Current.CancellationToken));
        Assert.Equal("Moto G84", session.DeviceName);
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            entry => entry.TargetId == user.Id && entry.Action == AuditActions.SignedIn,
            TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Login_WithWrongPasswordOrUnknownEmail_RespondsTheSame()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);

        var wrongPassword = await _client.LoginAsync(email, "senha que não é a certa");
        var unknownEmail = await _client.LoginAsync(AuthApi.NewEmail());

        var first = await wrongPassword.AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_credentials");
        var second = await unknownEmail.AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_credentials");
        Assert.Equal(first.GetProperty("detail").GetString(), second.GetProperty("detail").GetString());
        Assert.Equal(PropertyNames(first), PropertyNames(second));
    }

    [Fact]
    public async Task Login_AfterFiveWrongPasswords_IsLockedEvenWithTheRightPassword()
    {
        var email = AuthApi.NewEmail();
        var user = await factory.SeedActiveUserAsync(email);
        for (var attempt = 0; attempt < User.MaxFailedSignIns; attempt++)
        {
            await (await _client.LoginAsync(email, "senha que não é a certa")).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_credentials");
        }

        var response = await _client.LoginAsync(email);

        await response.AssertProblemAsync(HttpStatusCode.TooManyRequests, "auth.locked_out");
        Assert.InRange(response.Headers.RetryAfter!.Delta!.Value, TimeSpan.FromMinutes(14), User.LockoutDuration);
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            entry => entry.TargetId == user.Id && entry.Action == AuditActions.SignInLockedOut,
            TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Login_WithPendingAccount_ReturnsEmailNotVerifiedOnlyWithTheRightPassword()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);

        await (await _client.LoginAsync(email)).AssertProblemAsync(HttpStatusCode.Conflict, "account.email_not_verified");
        await (await _client.LoginAsync(email, "senha que não é a certa")).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_credentials");
    }

    [Fact]
    public async Task Login_WithSuspendedAccount_ReturnsSuspended()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        await factory.QueryAsync(db => db.Users
            .Where(user => user.Email == email)
            .ExecuteUpdateAsync(setters => setters.SetProperty(user => user.Status, UserStatus.Suspended), TestContext.Current.CancellationToken));

        var response = await _client.LoginAsync(email);

        await response.AssertProblemAsync(HttpStatusCode.Forbidden, "account.suspended");
    }

    [Fact]
    public async Task Refresh_ReturnsANewPairAndRetiresTheOldRefreshToken()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.RefreshAsync(auth.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.ReadTokensAsync();
        Assert.NotEqual(auth.RefreshToken, tokens.RefreshToken);
        Assert.Equal(600, tokens.ExpiresIn);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsAsync(HttpMethod.Get, ProtectedProbe, tokens.AccessToken)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.RefreshAsync(tokens.RefreshToken)).StatusCode);
    }

    [Fact]
    public async Task Refresh_WithATokenAlreadyUsed_EndsTheWholeSession()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        var rotated = await (await _client.RefreshAsync(auth.RefreshToken)).ReadTokensAsync();

        var reused = await _client.RefreshAsync(auth.RefreshToken);

        await reused.AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
        await (await _client.RefreshAsync(rotated.RefreshToken)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsAsync(HttpMethod.Get, ProtectedProbe, rotated.AccessToken)).StatusCode);
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            entry => entry.ActorId == auth.User.Id && entry.Action == AuditActions.RefreshReused,
            TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_ReturnsUnauthorized()
    {
        var response = await _client.RefreshAsync("token-que-nunca-existiu");

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
    }

    [Fact]
    public async Task Logout_OnThisDevice_KeepsTheOtherDevicesSignedIn()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        var phone = await (await _client.LoginAsync(email, deviceName: "Celular")).ReadAuthAsync();
        var tablet = await (await _client.LoginAsync(email, deviceName: "Tablet")).ReadAuthAsync();

        Assert.Equal(HttpStatusCode.NoContent, (await _client.LogoutAsync(phone.AccessToken, allDevices: false)).StatusCode);

        await (await _client.RefreshAsync(phone.RefreshToken)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsAsync(HttpMethod.Get, ProtectedProbe, phone.AccessToken)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.RefreshAsync(tablet.RefreshToken)).StatusCode);
    }

    [Fact]
    public async Task Logout_OnAllDevices_EndsEverySession()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        var phone = await (await _client.LoginAsync(email, deviceName: "Celular")).ReadAuthAsync();
        var tablet = await (await _client.LoginAsync(email, deviceName: "Tablet")).ReadAuthAsync();
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsAsync(HttpMethod.Get, ProtectedProbe, tablet.AccessToken)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.LogoutAsync(phone.AccessToken, allDevices: true)).StatusCode);

        await (await _client.RefreshAsync(tablet.RefreshToken)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsAsync(HttpMethod.Get, ProtectedProbe, tablet.AccessToken)).StatusCode);
    }

    [Fact]
    public async Task ListSessions_ShowsTheConnectedDevicesAndMarksTheCurrentOne()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        var phone = await (await _client.LoginAsync(email, deviceName: "Celular")).ReadAuthAsync();
        await _client.LoginAsync(email, deviceName: "Tablet");

        var response = await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me/sessions", phone.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sessions = await response.Content.ReadFromJsonAsync<List<SessionResponse>>(TestContext.Current.CancellationToken);
        Assert.Equal(["Celular", "Tablet"], sessions!.Select(session => session.DeviceName).Order());
        Assert.Equal("Celular", Assert.Single(sessions!, session => session.Current).DeviceName);
    }

    [Fact]
    public async Task RevokeSession_EndsTheOtherDevice()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        var phone = await (await _client.LoginAsync(email, deviceName: "Celular")).ReadAuthAsync();
        var tablet = await (await _client.LoginAsync(email, deviceName: "Tablet")).ReadAuthAsync();
        var tabletSessionId = await SessionIdOfAsync(phone.AccessToken, "Tablet");

        var response = await _client.SendAsAsync(HttpMethod.Delete, $"/api/v1/me/sessions/{tabletSessionId}", phone.AccessToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await (await _client.RefreshAsync(tablet.RefreshToken)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
        Assert.Equal(HttpStatusCode.OK, (await _client.RefreshAsync(phone.RefreshToken)).StatusCode);
    }

    [Fact]
    public async Task RevokeSession_OfAnotherPerson_ReturnsNotFound()
    {
        var mine = await factory.SignInNewAccountAsync(_client);
        var theirs = await factory.SignInNewAccountAsync(_client, "Outro aparelho");
        var theirSessionId = await SessionIdOfAsync(theirs.AccessToken, "Outro aparelho");

        var response = await _client.SendAsAsync(HttpMethod.Delete, $"/api/v1/me/sessions/{theirSessionId}", mine.AccessToken);

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "session.not_found");
        Assert.Equal(HttpStatusCode.OK, (await _client.RefreshAsync(theirs.RefreshToken)).StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/v1/me/sessions")]
    [InlineData("DELETE", "/api/v1/me/sessions/0199a0c4-3f2e-7c1a-9d7e-2a4b6c8d0e1f")]
    [InlineData("POST", "/api/v1/auth/logout")]
    public async Task AccountEndpoints_WithoutToken_ReturnUnauthorized(string method, string url)
    {
        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url), TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "unauthorized");
    }

    [Fact]
    public async Task RevokedSession_AfterTheApiRestarts_IsStillRejected()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.LogoutAsync(auth.AccessToken, allDevices: false)).StatusCode);

        using var restarted = factory.WithWebHostBuilder(_ => { });
        var response = await restarted.CreateClient().SendAsAsync(HttpMethod.Get, ProtectedProbe, auth.AccessToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ActiveSession_AfterTheApiRestarts_IsStillAccepted()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        using var restarted = factory.WithWebHostBuilder(_ => { });
        var response = await restarted.CreateClient().SendAsAsync(HttpMethod.Get, ProtectedProbe, auth.AccessToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> SessionIdOfAsync(string accessToken, string deviceName)
    {
        var response = await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me/sessions", accessToken);
        var sessions = await response.Content.ReadFromJsonAsync<List<SessionResponse>>(TestContext.Current.CancellationToken);
        return sessions!.Single(session => session.DeviceName == deviceName).Id;
    }

    private static List<string> PropertyNames(JsonElement problem) =>
        problem.EnumerateObject().Select(property => property.Name).Where(name => name != "traceId").Order().ToList();
}
