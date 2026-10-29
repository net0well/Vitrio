using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.IntegrationTests.Accounts;

public sealed class RegistrationTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Register_WithNewEmail_CreatesPendingAccountAndSendsCode()
    {
        var email = AuthApi.NewEmail();

        var response = await _client.RegisterAsync(email);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var user = await factory.FindUserAsync(email);
        Assert.NotNull(user);
        Assert.Equal(UserStatus.PendingVerification, user.Status);
        Assert.Null(user.EmailVerifiedAt);
        Assert.Equal(AuthApi.TermsVersion, user.AcceptedTermsVersion);
        Assert.False(await factory.QueryAsync(db => db.Sessions.AnyAsync(session => session.UserId == user.Id, TestContext.Current.CancellationToken)));

        var message = Assert.IsType<VerificationCodeEmail>(Assert.Single(factory.Emails.SentTo(email)));
        Assert.Matches("^[0-9]{6}$", message.Code);
        Assert.Equal("Ana", message.FirstName);
    }

    [Fact]
    public async Task Register_StoresOnlyHashesOfThePasswordAndTheCode()
    {
        var email = AuthApi.NewEmail();

        await _client.RegisterAsync(email);

        var code = factory.Emails.LastCodeFor(email);
        var user = await factory.FindUserAsync(email);
        var codeHashes = await factory.QueryAsync(db => db.EmailCodes
            .Where(emailCode => emailCode.Email == email)
            .Select(emailCode => emailCode.CodeHash)
            .ToListAsync(TestContext.Current.CancellationToken));
        Assert.DoesNotContain(AuthApi.Password, user!.PasswordHash, StringComparison.Ordinal);
        Assert.All(codeHashes, hash => Assert.DoesNotContain(code, hash, StringComparison.Ordinal));
    }

    [Fact]
    public async Task VerifyEmail_WithTheCodeSent_ActivatesAccountAndOpensSession()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);

        var response = await _client.VerifyEmailAsync(email, factory.Emails.LastCodeFor(email));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.ReadAuthAsync();
        Assert.Equal(600, auth.ExpiresIn);
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
        Assert.Equal("Ana Lima", auth.User.DisplayName);
        Assert.Equal("AL", auth.User.Initials);
        Assert.Equal(email, auth.User.Email);
        Assert.Equal(new[] { Roles.Customer }, auth.User.Roles);
        Assert.False(auth.User.HasProviderProfile);

        var user = await factory.FindUserAsync(email);
        Assert.Equal(UserStatus.Active, user!.Status);
        Assert.NotNull(user.EmailVerifiedAt);

        var session = await factory.QueryAsync(db => db.Sessions.SingleAsync(session => session.UserId == user.Id, TestContext.Current.CancellationToken));
        Assert.Equal("Pixel 8", session.DeviceName);
        var refreshHash = await factory.QueryAsync(db => db.RefreshTokens
            .Where(token => token.SessionId == session.Id)
            .Select(token => token.TokenHash)
            .SingleAsync(TestContext.Current.CancellationToken));
        Assert.NotEqual(auth.RefreshToken, refreshHash);
    }

    [Fact]
    public async Task VerifyEmail_AccessTokenOpensProtectedEndpoints()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        var auth = await (await _client.VerifyEmailAsync(email, factory.Emails.LastCodeFor(email))).ReadAuthAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/probe/private");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_RecordsAuditWithTruncatedAddress()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        await _client.VerifyEmailAsync(email, factory.Emails.LastCodeFor(email));
        var user = await factory.FindUserAsync(email);

        var audit = await factory.QueryAsync(db => db.AuditEvents
            .AsNoTracking()
            .Where(entry => entry.TargetId == user!.Id)
            .OrderBy(entry => entry.OccurredAt)
            .ToListAsync(TestContext.Current.CancellationToken));

        Assert.Equal([AuditActions.AccountRegistered, AuditActions.EmailVerified], audit.Select(entry => entry.Action));
        Assert.All(audit, entry =>
        {
            Assert.Equal(user!.Id, entry.ActorId);
            Assert.Matches(@"^10\.\d+\.\d+\.0/24$", entry.IpPrefix);
            Assert.False(string.IsNullOrWhiteSpace(entry.TraceId));
        });
    }

    [Fact]
    public async Task VerifyEmail_WithWrongCode_ReturnsInvalidCode()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);

        var response = await _client.VerifyEmailAsync(email, WrongCode(factory.Emails.LastCodeFor(email)));

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
        Assert.Equal(UserStatus.PendingVerification, (await factory.FindUserAsync(email))!.Status);
    }

    [Fact]
    public async Task VerifyEmail_WithAnotherPassword_ReturnsInvalidCodeAndCountsTheAttempt()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);

        var response = await _client.VerifyEmailAsync(email, factory.Emails.LastCodeFor(email), "outra frase de senha");

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
        Assert.Equal(UserStatus.PendingVerification, (await factory.FindUserAsync(email))!.Status);
        var attempts = await factory.QueryAsync(db => db.EmailCodes
            .Where(code => code.Email == email)
            .Select(code => code.Attempts)
            .SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task VerifyEmail_AfterFiveWrongAttempts_RejectsTheRightCode()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        var code = factory.Emails.LastCodeFor(email);
        for (var attempt = 0; attempt < EmailCode.MaxAttempts; attempt++)
        {
            await _client.VerifyEmailAsync(email, WrongCode(code));
        }

        var response = await _client.VerifyEmailAsync(email, code);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
    }

    [Fact]
    public async Task VerifyEmail_WithCodeAlreadyUsed_ReturnsInvalidCode()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        var code = factory.Emails.LastCodeFor(email);
        await _client.VerifyEmailAsync(email, code);

        var response = await _client.VerifyEmailAsync(email, code);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
    }

    [Fact]
    public async Task VerifyEmail_ForUnknownEmail_ReturnsInvalidCode()
    {
        var response = await _client.VerifyEmailAsync(AuthApi.NewEmail(), "123456");

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
    }

    [Fact]
    public async Task Register_WithEmailOfActiveAccount_RespondsLikeANewEmailAndWarnsTheOwner()
    {
        var email = AuthApi.NewEmail();
        var owner = await factory.SeedActiveUserAsync(email);

        var forExisting = await _client.RegisterAsync(email, "Outra Pessoa", "outra senha bem longa");
        var forNew = await factory.CreateAuthClient().RegisterAsync(AuthApi.NewEmail());

        Assert.Equal(forNew.StatusCode, forExisting.StatusCode);
        Assert.Equal(
            await forNew.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
            await forExisting.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var user = await factory.FindUserAsync(email);
        Assert.Equal(owner.DisplayName, user!.DisplayName);
        Assert.Equal(owner.PasswordHash, user.PasswordHash);
        Assert.Equal(1, await factory.QueryAsync(db => db.Users.CountAsync(account => account.Email == email, TestContext.Current.CancellationToken)));

        var notice = Assert.IsType<RegistrationAttemptEmail>(Assert.Single(factory.Emails.SentTo(email)));
        Assert.Equal("Bruno", notice.FirstName);
    }

    [Theory]
    [InlineData("curta1234")]
    [InlineData("1234567890")]
    [InlineData("Senha12345")]
    public async Task Register_WithWeakPassword_ReturnsValidationErrorOnPassword(string password)
    {
        var email = AuthApi.NewEmail();

        var response = await _client.RegisterAsync(email, password: password);

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.True(problem.GetProperty("errors").TryGetProperty("password", out _));
        Assert.Null(await factory.FindUserAsync(email));
        Assert.Empty(factory.Emails.SentTo(email));
    }

    [Fact]
    public async Task Register_WithPasswordEqualToEmail_ReturnsValidationErrorOnPassword()
    {
        var email = AuthApi.NewEmail();

        var response = await _client.RegisterAsync(email, password: email.ToUpperInvariant());

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.True(problem.GetProperty("errors").TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Register_WithoutAcceptingTheTerms_ReturnsValidationError()
    {
        var response = await _client.RegisterAsync(AuthApi.NewEmail(), acceptedTermsVersion: null);

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.True(problem.GetProperty("errors").TryGetProperty("acceptedTermsVersion", out _));
    }

    [Fact]
    public async Task Register_WithContactInTheName_ReturnsValidationErrorOnDisplayName()
    {
        var response = await _client.RegisterAsync(AuthApi.NewEmail(), displayName: "Ana 11 98765-4321");

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.True(problem.GetProperty("errors").TryGetProperty("displayName", out _));
    }

    [Fact]
    public async Task Register_WhenTheEmailCannotBeSent_ReturnsServiceUnavailableAndKeepsTheCode()
    {
        var email = AuthApi.NewEmail();
        factory.Emails.FailDeliveriesTo(email);

        var response = await _client.RegisterAsync(email);

        await response.AssertProblemAsync(HttpStatusCode.ServiceUnavailable, "email.unavailable");
        Assert.Equal(TimeSpan.FromSeconds(60), response.Headers.RetryAfter?.Delta);
        Assert.Equal(UserStatus.PendingVerification, (await factory.FindUserAsync(email))!.Status);
    }

    [Fact]
    public async Task ResendCode_RightAfterAnotherCode_ReturnsTooManyRequestsWithRetryAfter()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);

        var response = await _client.ResendCodeAsync(email);

        await response.AssertProblemAsync(HttpStatusCode.TooManyRequests, "auth.too_many_codes");
        Assert.InRange(response.Headers.RetryAfter!.Delta!.Value, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
        Assert.Single(factory.Emails.SentTo(email));
    }

    [Fact]
    public async Task ResendCode_ForUnknownEmail_ReturnsAcceptedWithoutSendingAnything()
    {
        var email = AuthApi.NewEmail();

        var response = await _client.ResendCodeAsync(email);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(factory.Emails.SentTo(email));
    }

    [Fact]
    public async Task ResendCode_WithoutPurpose_ReturnsValidationError()
    {
        var response = await _client.PostAsync(
            "/api/v1/auth/resend-code",
            new StringContent(JsonSerializer.Serialize(new { email = AuthApi.NewEmail() }), System.Text.Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
    }

    private static string WrongCode(string code) => code == "000000" ? "111111" : "000000";
}
