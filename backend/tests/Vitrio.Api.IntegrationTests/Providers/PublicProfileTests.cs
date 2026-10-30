using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Api.IntegrationTests.Providers;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class PublicProfileTests(ApiFactory factory)
{
    private const string Provider = ProviderApi.Provider;

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task PublicProfile_OfAPublishedProvider_ShowsTheShowcase()
    {
        var (accessToken, id) = await PublishedProviderAsync();

        var response = await _client.GetAsync(Public(id), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadApiAsync<ProviderPublicResponse>();
        Assert.Equal("Carlos Eletricista", profile.DisplayName);
        Assert.Equal("Jaboticabal", profile.City.Name);
        Assert.Equal("Elétrica", profile.Categories[0].Name);
        Assert.True(profile.IsNew);
        Assert.Null(profile.Rating);
        Assert.Equal("+5516991234567", profile.WhatsAppPhone);
        Assert.Equal($"https://vitrio.app/p/{id}", profile.ShareUrl);
        Assert.NotNull(accessToken);
    }

    [Fact]
    public async Task PublicProfile_NeverExposesTheBaseAddressOrCoordinates_AndRoundsTheDistance()
    {
        var (_, id) = await PublishedProviderAsync();

        var response = await _client.GetAsync($"{Public(id)}?lat=-21.30&lng=-48.30", TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("Rui Barbosa", json, StringComparison.Ordinal);
        Assert.DoesNotContain("14870", json, StringComparison.Ordinal);
        Assert.DoesNotContain("21.25", json, StringComparison.Ordinal);
        Assert.DoesNotContain("atitude", json, StringComparison.Ordinal);
        Assert.DoesNotContain("cnpj", json, StringComparison.OrdinalIgnoreCase);
        var profile = await response.Content.ReadApiAsync<ProviderPublicResponse>();
        Assert.NotNull(profile.DistanceKm);
        Assert.Equal(Math.Round(profile.DistanceKm!.Value, 1), profile.DistanceKm.Value);
    }

    [Fact]
    public async Task PublicProfile_WithWhatsAppOff_HidesThePhone()
    {
        var (accessToken, id) = await PublishedProviderAsync();
        await _client.SendJsonAsAsync(HttpMethod.Patch, Provider, accessToken, new { whatsAppEnabled = false });

        await Eventually.SucceedsAsync(async () =>
            (await (await _client.GetAsync(Public(id), TestContext.Current.CancellationToken)).Content.ReadApiAsync<ProviderPublicResponse>())
                .WhatsAppPhone is null);
    }

    [Fact]
    public async Task PublicProfile_OfADraft_IsNotFound()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var id = (await _client.GetProviderAsync(accessToken)).Id;

        var response = await _client.GetAsync(Public(id), TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "provider.not_found");
    }

    [Fact]
    public async Task PublicProfile_AfterPause_IsNotFoundWithoutWaitingForTheCache()
    {
        var (accessToken, id) = await PublishedProviderAsync();
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync(Public(id), TestContext.Current.CancellationToken)).StatusCode);

        await _client.SendAsAsync(HttpMethod.Post, $"{Provider}/pause", accessToken);

        var elapsed = await Eventually.SucceedsAsync(async () =>
            (await _client.GetAsync(Public(id), TestContext.Current.CancellationToken)).StatusCode == HttpStatusCode.NotFound);
        Assert.True(elapsed < TimeSpan.FromSeconds(5), $"Took {elapsed}");
    }

    [Fact]
    public async Task PublicProfile_AfterTheOwnerChangesAbout_ShowsTheNewTextWithinSeconds()
    {
        var (accessToken, id) = await PublishedProviderAsync();
        await _client.GetAsync(Public(id), TestContext.Current.CancellationToken);
        const string About = ProviderApi.About + " Atendo também condomínios e comércios.";

        await _client.SendJsonAsAsync(HttpMethod.Patch, Provider, accessToken, new { about = About });

        var elapsed = await Eventually.SucceedsAsync(async () =>
            (await (await _client.GetAsync(Public(id), TestContext.Current.CancellationToken)).Content.ReadApiAsync<ProviderPublicResponse>())
                .About == About);
        Assert.True(elapsed < TimeSpan.FromSeconds(5), $"Took {elapsed}");
    }

    [Fact]
    public async Task PublicProfile_AvailableTodayTurnedOnYesterday_HidesTheBadgeWithoutAnyJob()
    {
        var (accessToken, id) = await PublishedProviderAsync();
        await _client.SendJsonAsAsync(HttpMethod.Put, $"{Provider}/available-today", accessToken, new { value = true });
        await Eventually.SucceedsAsync(async () =>
            (await (await _client.GetAsync(Public(id), TestContext.Current.CancellationToken)).Content.ReadApiAsync<ProviderPublicResponse>())
                .Badges.AvailableToday);

        factory.Clock.Advance(TimeSpan.FromDays(1));

        var profile = await (await _client.GetAsync(Public(id), TestContext.Current.CancellationToken)).Content.ReadApiAsync<ProviderPublicResponse>();
        Assert.False(profile.Badges.AvailableToday);
    }

    [Fact]
    public async Task DeletingTheAccount_RemovesTheShowcase()
    {
        var email = AuthApi.NewEmail();
        await factory.SeedActiveUserAsync(email);
        var auth = await (await _client.LoginAsync(email)).ReadAuthAsync();
        await _client.SendJsonAsAsync(HttpMethod.Post, Provider, auth.AccessToken, new { displayName = "Rita Diarista", headline = "Diarista" });
        var accessToken = (await (await _client.RefreshAsync(auth.RefreshToken)).ReadTokensAsync()).AccessToken;
        await _client.CompleteAsync(accessToken);
        await _client.SendAsAsync(HttpMethod.Post, $"{Provider}/publish", accessToken);
        var id = (await _client.GetProviderAsync(accessToken)).Id;
        await _client.GetAsync(Public(id), TestContext.Current.CancellationToken);

        await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", accessToken);

        await Eventually.SucceedsAsync(async () =>
            (await _client.GetAsync(Public(id), TestContext.Current.CancellationToken)).StatusCode == HttpStatusCode.NotFound);
        await Eventually.SucceedsAsync(async () =>
            await factory.QueryAsync(db => db.Providers.AsNoTracking().AnyAsync(
                provider => provider.Id == id && provider.Status == ProviderStatus.Removed,
                TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task PublicProfile_WithLatitudeWithoutLongitude_ReturnsBadRequest()
    {
        var response = await _client.GetAsync($"{Public(Guid.NewGuid())}?lat=-21.3", TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
    }

    private static string Public(Guid id) => $"/api/v1/providers/{id}";

    private async Task<(string AccessToken, Guid Id)> PublishedProviderAsync()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        await _client.CompleteAsync(accessToken);
        var published = await _client.SendAsAsync(HttpMethod.Post, $"{Provider}/publish", accessToken);
        Assert.Equal(HttpStatusCode.OK, published.StatusCode);
        return (accessToken, (await _client.GetProviderAsync(accessToken)).Id);
    }
}
