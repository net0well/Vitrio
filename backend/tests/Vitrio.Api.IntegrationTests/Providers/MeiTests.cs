using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.RecheckMeiVerifications;
using Vitrio.Domain.Providers;

namespace Vitrio.Api.IntegrationTests.Providers;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class MeiTests(ApiFactory factory)
{
    private const string Verification = $"{ProviderApi.Provider}/mei-verification";

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Verify_ActiveMei_ShowsTheBadgeOnThePublicProfile()
    {
        var (accessToken, id) = await PublishedProviderAsync();
        var cnpj = FakeCnpjLookup.NewCnpj();
        factory.Cnpj.AnswerActiveMei(cnpj);

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Verification, accessToken, new { cnpj = Formatted(cnpj) });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadApiAsync<MeiVerificationResponse>();
        Assert.Equal(MeiStatus.Verified, result.Status);
        Assert.Equal("CARLOS ELETRICISTA", result.CompanyName);
        await Eventually.SucceedsAsync(async () => (await PublicAsync(id)).Badges.MeiVerified);
    }

    [Fact]
    public async Task Verify_CnpjThatIsNotMei_FailsWithAReadableReason()
    {
        var (accessToken, id) = await PublishedProviderAsync();
        var cnpj = FakeCnpjLookup.NewCnpj();
        factory.Cnpj.AnswerNotMei(cnpj);

        var result = await (await _client.SendJsonAsAsync(HttpMethod.Post, Verification, accessToken, new { cnpj })).Content.ReadApiAsync<MeiVerificationResponse>();

        Assert.Equal(MeiStatus.Failed, result.Status);
        Assert.Equal("CNPJ não é optante pelo MEI.", result.Reason);
        Assert.False((await PublicAsync(id)).Badges.MeiVerified);
    }

    [Fact]
    public async Task Verify_CnpjOfAnotherShowcase_ReturnsConflict()
    {
        var cnpj = FakeCnpjLookup.NewCnpj();
        factory.Cnpj.AnswerActiveMei(cnpj);
        var owner = await factory.SignInAsProviderAsync(_client);
        await _client.SendJsonAsAsync(HttpMethod.Post, Verification, owner, new { cnpj });
        var other = await factory.SignInAsProviderAsync(_client, "Outra Pessoa");

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Verification, other, new { cnpj });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "provider.cnpj_in_use");
    }

    [Fact]
    public async Task Verify_WithWrongCheckDigit_ReturnsBadRequest()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Verification, accessToken, new { cnpj = "11.222.333/0001-82" });

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.Contains("CNPJ", problem.GetProperty("errors").GetProperty("cnpj")[0].GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Verify_WhenTheLookupIsDown_StaysPendingWithAccepted()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var cnpj = FakeCnpjLookup.NewCnpj();
        factory.Cnpj.AnswerUnavailable(cnpj);

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Verification, accessToken, new { cnpj });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(MeiStatus.Pending, (await response.Content.ReadApiAsync<MeiVerificationResponse>()).Status);
    }

    [Fact]
    public async Task Revalidation_ThatLosesTheBadge_HidesItRightAway()
    {
        var (accessToken, id) = await PublishedProviderAsync();
        var cnpj = FakeCnpjLookup.NewCnpj();
        factory.Cnpj.AnswerActiveMei(cnpj);
        await _client.SendJsonAsAsync(HttpMethod.Post, Verification, accessToken, new { cnpj });
        await Eventually.SucceedsAsync(async () => (await PublicAsync(id)).Badges.MeiVerified);

        factory.Cnpj.AnswerNotMei(cnpj);
        factory.Clock.Advance(TimeSpan.FromDays(31));
        await RecheckAsync();

        var elapsed = await Eventually.SucceedsAsync(async () => !(await PublicAsync(id)).Badges.MeiVerified);
        Assert.True(elapsed < TimeSpan.FromSeconds(5), $"Took {elapsed}");
        Assert.Equal(MeiStatus.Lost, await StatusOfAsync(id));
    }

    [Fact]
    public async Task PendingForMoreThanThreeDays_FailsAndRaisesTheEvent()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var id = (await _client.GetProviderAsync(accessToken)).Id;
        var cnpj = FakeCnpjLookup.NewCnpj();
        factory.Cnpj.AnswerUnavailable(cnpj);
        await _client.SendJsonAsAsync(HttpMethod.Post, Verification, accessToken, new { cnpj });

        factory.Clock.Advance(TimeSpan.FromDays(3) + TimeSpan.FromHours(1));
        await RecheckAsync();

        var stored = await factory.QueryAsync(db => db.Providers.AsNoTracking().Where(provider => provider.Id == id)
            .Select(provider => new { provider.Mei!.Status, provider.Mei.ReasonCode })
            .SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(MeiStatus.Failed, stored.Status);
        Assert.Equal(MeiReasons.LookupUnavailable, stored.ReasonCode);
        var events = await factory.QueryAsync(db => db.OutboxMessages.AsNoTracking()
            .Where(message => message.Type == "mei.verification_failed")
            .Select(message => message.Payload)
            .ToListAsync(TestContext.Current.CancellationToken));
        Assert.Contains(events, payload => payload.Contains(id.ToString(), StringComparison.Ordinal) && payload.Contains("\"fromPending\": true", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DeletingTheAccount_FreesTheCnpjForAnotherShowcase()
    {
        var cnpj = FakeCnpjLookup.NewCnpj();
        factory.Cnpj.AnswerActiveMei(cnpj);
        var owner = await factory.SignInAsProviderAsync(_client);
        var ownerId = (await _client.GetProviderAsync(owner)).Id;
        await _client.SendJsonAsAsync(HttpMethod.Post, Verification, owner, new { cnpj });

        await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", owner);
        await Eventually.SucceedsAsync(async () => await factory.QueryAsync(db =>
            db.Providers.AsNoTracking().AnyAsync(provider => provider.Id == ownerId && provider.Status == ProviderStatus.Removed, TestContext.Current.CancellationToken)));

        var other = await factory.SignInAsProviderAsync(_client, "Nova Dona");
        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Verification, other, new { cnpj });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Verify_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync(Verification, null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string Formatted(string cnpj) => $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}";

    private async Task RecheckAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ISender>()
            .Send(new RecheckMeiVerificationsCommand(200, TimeSpan.Zero), TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
    }

    private Task<MeiStatus> StatusOfAsync(Guid providerId) =>
        factory.QueryAsync(db => db.Providers.AsNoTracking().Where(provider => provider.Id == providerId)
            .Select(provider => provider.Mei!.Status)
            .SingleAsync(TestContext.Current.CancellationToken));

    private async Task<ProviderPublicResponse> PublicAsync(Guid id) =>
        await (await _client.GetAsync($"/api/v1/providers/{id}", TestContext.Current.CancellationToken)).Content.ReadApiAsync<ProviderPublicResponse>();

    private async Task<(string AccessToken, Guid Id)> PublishedProviderAsync()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        await _client.CompleteAsync(accessToken);
        await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/publish", accessToken);
        return (accessToken, (await _client.GetProviderAsync(accessToken)).Id);
    }
}
