using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.IntegrationTests.Accounts;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class RegistrationClockTests(ApiFactory factory)
{
    private static readonly TimeSpan PastResendInterval = EmailCode.ResendInterval + TimeSpan.FromSeconds(1);

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task VerifyEmail_WithCodeOlderThanTenMinutes_ReturnsInvalidCode()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        factory.Clock.Advance(TimeSpan.FromMinutes(11));

        var response = await _client.VerifyEmailAsync(email, factory.Emails.LastCodeFor(email));

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
    }

    [Fact]
    public async Task ResendCode_SixthCodeInOneHour_ReturnsTooManyRequestsAndSendsNothing()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        for (var code = 2; code <= EmailCode.MaxIssuedPerWindow; code++)
        {
            factory.Clock.Advance(PastResendInterval);
            Assert.Equal(HttpStatusCode.Accepted, (await _client.ResendCodeAsync(email)).StatusCode);
        }

        factory.Clock.Advance(PastResendInterval);
        var response = await _client.ResendCodeAsync(email);

        await response.AssertProblemAsync(HttpStatusCode.TooManyRequests, "auth.too_many_codes");
        var expectedWait = EmailCode.IssueWindow - (PastResendInterval * EmailCode.MaxIssuedPerWindow);
        Assert.InRange(response.Headers.RetryAfter!.Delta!.Value, expectedWait - TimeSpan.FromSeconds(5), expectedWait + TimeSpan.FromSeconds(5));
        Assert.Equal(EmailCode.MaxIssuedPerWindow, factory.Emails.SentTo(email).Count);
    }

    [Fact]
    public async Task ResendCode_SendsANewCodeAndInvalidatesThePreviousOne()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        factory.Clock.Advance(PastResendInterval);

        Assert.Equal(HttpStatusCode.Accepted, (await _client.ResendCodeAsync(email)).StatusCode);

        var codes = await factory.QueryAsync(db => db.EmailCodes
            .AsNoTracking()
            .Where(code => code.Email == email)
            .OrderBy(code => code.CreatedAt)
            .ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, codes.Count);
        Assert.NotNull(codes[0].ConsumedAt);
        Assert.Null(codes[1].ConsumedAt);
        Assert.Equal(HttpStatusCode.OK, (await _client.VerifyEmailAsync(email, factory.Emails.LastCodeFor(email))).StatusCode);
    }

    [Fact]
    public async Task Register_AgainBeforeConfirming_UpdatesTheAccountAndSendsANewCode()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        factory.Clock.Advance(PastResendInterval);

        var response = await _client.RegisterAsync(email, "Ana Souza", "outra frase de senha");

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(2, factory.Emails.SentTo(email).OfType<VerificationCodeEmail>().Count());
        var verification = await _client.VerifyEmailAsync(email, factory.Emails.LastCodeFor(email), "outra frase de senha");
        Assert.Equal("Ana Souza", (await verification.ReadAuthAsync()).User.DisplayName);
    }

    [Fact]
    public async Task VerifyEmail_AfterSomeoneRegistersAgain_RejectsThePreviousPassword()
    {
        var email = AuthApi.NewEmail();
        await _client.RegisterAsync(email);
        factory.Clock.Advance(PastResendInterval);
        await factory.CreateAuthClient().RegisterAsync(email, "Outra Pessoa", "senha de quem tentou");

        var response = await _client.VerifyEmailAsync(email, factory.Emails.LastCodeFor(email), AuthApi.Password);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "auth.invalid_code");
        Assert.Equal(UserStatus.PendingVerification, (await factory.FindUserAsync(email))!.Status);
    }

    [Fact]
    public async Task Register_WithEmailOfActiveAccount_WarnsTheOwnerAtMostOncePerDay()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);

        await _client.RegisterAsync(email);
        factory.Clock.Advance(PastResendInterval);
        Assert.Equal(HttpStatusCode.Accepted, (await _client.RegisterAsync(email)).StatusCode);
        Assert.Single(factory.Emails.SentTo(email));

        factory.Clock.Advance(TimeSpan.FromHours(24));
        Assert.Equal(HttpStatusCode.Accepted, (await _client.RegisterAsync(email)).StatusCode);

        Assert.Equal(2, factory.Emails.SentTo(email).OfType<RegistrationAttemptEmail>().Count());
    }
}
