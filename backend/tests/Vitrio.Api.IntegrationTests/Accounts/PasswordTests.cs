using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.IntegrationTests.Accounts;

public sealed class PasswordTests(ApiFactory factory)
{
    private const string NewPassword = "janela nova de vidro";
    private const string ProtectedProbe = "/api/v1/probe/private";

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task ForgotPassword_ForActiveAccount_SendsAResetCode()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);

        var response = await _client.ForgotPasswordAsync(email);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var message = Assert.IsType<PasswordResetCodeEmail>(Assert.Single(factory.Emails.SentTo(email)));
        Assert.Matches("^[0-9]{6}$", message.Code);
        Assert.Equal("Bruno", message.FirstName);
    }

    [Fact]
    public async Task ForgotPassword_ForUnknownEmail_RespondsAcceptedAndSendsNothing()
    {
        var unknown = AuthApi.NewEmail();

        var response = await _client.ForgotPasswordAsync(unknown);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(factory.Emails.SentTo(unknown));
    }

    [Fact]
    public async Task ResetPassword_WithTheCode_ChangesThePasswordAndEndsEverySession()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        var oldDevice = await (await _client.LoginAsync(email, deviceName: "Celular antigo")).ReadAuthAsync();
        await _client.ForgotPasswordAsync(email);

        var response = await _client.ResetPasswordAsync(email, factory.Emails.LastResetCodeFor(email), NewPassword, "Celular novo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.ReadAuthAsync();
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsAsync(HttpMethod.Get, ProtectedProbe, auth.AccessToken)).StatusCode);
        await (await _client.RefreshAsync(oldDevice.RefreshToken)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsAsync(HttpMethod.Get, ProtectedProbe, oldDevice.AccessToken)).StatusCode);
        await (await _client.LoginAsync(email)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_credentials");
        Assert.Equal(HttpStatusCode.OK, (await _client.LoginAsync(email, NewPassword)).StatusCode);

        var notice = Assert.Single(factory.Emails.SentTo(email).OfType<PasswordChangedEmail>());
        Assert.Equal("Bruno", notice.FirstName);
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            entry => entry.TargetId == auth.User.Id && entry.Action == AuditActions.PasswordReset,
            TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task ResetPassword_UnlocksALockedAccount()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        for (var attempt = 0; attempt < User.MaxFailedSignIns; attempt++)
        {
            await _client.LoginAsync(email, "senha que não é a certa");
        }

        await (await _client.LoginAsync(email)).AssertProblemAsync(HttpStatusCode.TooManyRequests, "auth.locked_out");
        await _client.ForgotPasswordAsync(email);
        await _client.ResetPasswordAsync(email, factory.Emails.LastResetCodeFor(email), NewPassword);

        Assert.Equal(HttpStatusCode.OK, (await factory.CreateAuthClient().LoginAsync(email, NewPassword)).StatusCode);
    }

    [Theory]
    [InlineData("curta1234")]
    [InlineData("1234567890")]
    public async Task ResetPassword_WithWeakPassword_ReturnsValidationErrorOnNewPassword(string weakPassword)
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        await _client.ForgotPasswordAsync(email);

        var response = await _client.ResetPasswordAsync(email, factory.Emails.LastResetCodeFor(email), weakPassword);

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.True(problem.GetProperty("errors").TryGetProperty("newPassword", out _));
    }

    [Fact]
    public async Task ResetPassword_WithWrongCode_ReturnsInvalidCodeAndKeepsThePassword()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        await _client.ForgotPasswordAsync(email);
        var code = factory.Emails.LastResetCodeFor(email);

        var response = await _client.ResetPasswordAsync(email, code == "000000" ? "111111" : "000000", NewPassword);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
        Assert.Equal(HttpStatusCode.OK, (await _client.LoginAsync(email)).StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ForUnknownEmail_ReturnsInvalidCode()
    {
        var response = await _client.ResetPasswordAsync(AuthApi.NewEmail(), "123456", NewPassword);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
    }

    [Fact]
    public async Task ChangePassword_WithTheCurrentPassword_KeepsThisDeviceAndEndsTheOthers()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        var phone = await (await _client.LoginAsync(email, deviceName: "Celular")).ReadAuthAsync();
        var tablet = await (await _client.LoginAsync(email, deviceName: "Tablet")).ReadAuthAsync();

        var response = await _client.ChangePasswordAsync(phone.AccessToken, AuthApi.Password, NewPassword);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.RefreshAsync(phone.RefreshToken)).StatusCode);
        await (await _client.RefreshAsync(tablet.RefreshToken)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
        Assert.Equal(HttpStatusCode.OK, (await _client.LoginAsync(email, NewPassword)).StatusCode);
        Assert.Single(factory.Emails.SentTo(email).OfType<PasswordChangedEmail>());
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ReturnsForbidden()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.ChangePasswordAsync(auth.AccessToken, "senha que não é a certa", NewPassword);

        await response.AssertProblemAsync(HttpStatusCode.Forbidden, "auth.invalid_credentials");
    }

    [Fact]
    public async Task ChangePassword_WithFiveWrongCurrentPasswords_LocksTheAccount()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        for (var attempt = 0; attempt < User.MaxFailedSignIns; attempt++)
        {
            await (await _client.ChangePasswordAsync(auth.AccessToken, "senha que não é a certa", NewPassword))
                .AssertProblemAsync(HttpStatusCode.Forbidden, "auth.invalid_credentials");
        }

        var response = await _client.ChangePasswordAsync(auth.AccessToken, AuthApi.Password, NewPassword);

        await response.AssertProblemAsync(HttpStatusCode.TooManyRequests, "auth.locked_out");
        await (await _client.LoginAsync(auth.User.Email)).AssertProblemAsync(HttpStatusCode.TooManyRequests, "auth.locked_out");
    }

    [Fact]
    public async Task ChangePassword_ToAWeakPasswordOrTheEmail_ReturnsValidationErrorOnNewPassword()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var weak = await _client.ChangePasswordAsync(auth.AccessToken, AuthApi.Password, "curta1234");
        var sameAsEmail = await _client.ChangePasswordAsync(auth.AccessToken, AuthApi.Password, auth.User.Email);

        foreach (var response in new[] { weak, sameAsEmail })
        {
            var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
            Assert.True(problem.GetProperty("errors").TryGetProperty("newPassword", out _));
        }
    }

    [Fact]
    public async Task ChangePassword_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Put, "/api/v1/me/password"), TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "unauthorized");
    }
}
