using System.Net;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.IntegrationTests.Accounts;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class SignInClockTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Login_AfterTheLockoutEnds_SignsIn()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        for (var attempt = 0; attempt < User.MaxFailedSignIns; attempt++)
        {
            await _client.LoginAsync(email, "senha que não é a certa");
        }

        factory.Clock.Advance(User.LockoutDuration + TimeSpan.FromSeconds(1));

        Assert.Equal(HttpStatusCode.OK, (await _client.LoginAsync(email)).StatusCode);
    }

    [Fact]
    public async Task Refresh_UsedWithinSixtyDays_KeepsTheSessionButExpiresAfterSixtyIdleDays()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        factory.Clock.Advance(TimeSpan.FromDays(59));

        var renewed = await _client.RefreshAsync(auth.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, renewed.StatusCode);
        factory.Clock.Advance(TimeSpan.FromDays(61));
        var expired = await _client.RefreshAsync((await renewed.ReadTokensAsync()).RefreshToken);
        await expired.AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
    }

    [Fact]
    public async Task Refresh_MoreThanOneYearAfterLogin_ReturnsUnauthorized()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        var refreshToken = auth.RefreshToken;
        for (var renewal = 0; renewal < 6; renewal++)
        {
            factory.Clock.Advance(TimeSpan.FromDays(59));
            var renewed = await _client.RefreshAsync(refreshToken);
            Assert.Equal(HttpStatusCode.OK, renewed.StatusCode);
            refreshToken = (await renewed.ReadTokensAsync()).RefreshToken;
        }

        factory.Clock.Advance(TimeSpan.FromDays(12));
        var response = await _client.RefreshAsync(refreshToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
    }
}
