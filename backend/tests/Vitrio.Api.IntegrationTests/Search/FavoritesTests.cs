using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Providers;
using Vitrio.Application.Favorites.Common;
using Vitrio.Application.Providers.Common;

namespace Vitrio.Api.IntegrationTests.Search;

[Collection(nameof(ClockSensitiveCollection))]
public sealed class FavoritesTests(ApiFactory factory)
{
    private const string Favorites = "/api/v1/me/favorites";

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Favorite_SavedThenRemoved_AppearsAndDisappearsFromTheList()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);

        var saved = await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);
        var afterSaving = await ListAsync(customer.AccessToken);
        var removed = await _client.SendAsAsync(HttpMethod.Delete, Favorite(provider.Id), customer.AccessToken);
        var afterRemoving = await ListAsync(customer.AccessToken);

        Assert.Equal(HttpStatusCode.NoContent, saved.StatusCode);
        var item = Assert.Single(afterSaving.Items);
        Assert.Equal(provider.Id, item.Id);
        Assert.Equal("Carlos Menezes", item.DisplayName);
        Assert.Null(item.DistanceKm);
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.Empty(afterRemoving.Items);
    }

    [Fact]
    public async Task Favorite_SavedTwiceOrRemovedWhenMissing_RespondsNoContentAndKeepsOne()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);

        var first = await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);
        var second = await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);
        var missing = await _client.SendAsAsync(HttpMethod.Delete, Favorite(Guid.NewGuid()), customer.AccessToken);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, missing.StatusCode);
        Assert.Single((await ListAsync(customer.AccessToken)).Items);
    }

    [Fact]
    public async Task Favorite_SavedAtTheSameTimeOnTwoDevices_KeepsOne()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);

        var responses = await Task.WhenAll(
            _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken),
            _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken),
            _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode));
        Assert.Equal(1, await factory.QueryAsync(db => db.Favorites.CountAsync(
            favorite => favorite.UserId == customer.User.Id,
            TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Favorite_RemovedAtTheSameTimeOnSeveralDevices_RespondsNoContentToAll()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);
        await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);

        var responses = await Task.WhenAll(Enumerable.Range(0, 6)
            .Select(_ => _client.SendAsAsync(HttpMethod.Delete, Favorite(provider.Id), customer.AccessToken)));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode));
        Assert.Empty((await ListAsync(customer.AccessToken)).Items);
    }

    [Fact]
    public async Task Favorite_OfTheOwnShowcase_ReturnsNotFound()
    {
        var provider = await PublishAsync();

        var response = await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), provider.AccessToken);

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "provider.not_found");
    }

    [Fact]
    public async Task Favorite_OfAPausedOrUnknownShowcase_ReturnsNotFound()
    {
        var paused = await PublishAsync();
        Assert.True((await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/pause", paused.AccessToken)).IsSuccessStatusCode);
        var customer = await factory.SignInNewAccountAsync(_client);

        var pausedResponse = await _client.SendAsAsync(HttpMethod.Put, Favorite(paused.Id), customer.AccessToken);
        var unknownResponse = await _client.SendAsAsync(HttpMethod.Put, Favorite(Guid.NewGuid()), customer.AccessToken);

        await pausedResponse.AssertProblemAsync(HttpStatusCode.NotFound, "provider.not_found");
        await unknownResponse.AssertProblemAsync(HttpStatusCode.NotFound, "provider.not_found");
    }

    [Fact]
    public async Task Favorite_BeyondTwoHundred_ReturnsConflict()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);
        var owner = await factory.SignInNewAccountAsync(_client);
        await factory.QueryAsync(db => db.Database.ExecuteSqlAsync(
            $"""
            with removed as (
              insert into providers (id, user_id, display_name, headline, whats_app_enabled, status, location_approximate,
                service_radius_meters, available_today, handles_emergencies, review_count, completed_orders,
                response_sample_size, late_cancellations90d, created_at, updated_at)
              select gen_random_uuid(), {owner.User.Id}, 'Vitrine ' || n, 'Eletricista', false, 'Removed', false,
                1000, false, false, 0, 0, 0, 0, now(), now()
              from generate_series(1, 200) n
              returning id)
            insert into favorites (id, user_id, provider_id, created_at)
            select gen_random_uuid(), {customer.User.Id}, id, now() from removed
            """,
            TestContext.Current.CancellationToken));

        var response = await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "favorite.limit_reached");
    }

    [Fact]
    public async Task Favorite_OfAShowcaseThatPauses_LeavesTheListAndComesBackWhenPublished()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);
        await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);

        await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/pause", provider.AccessToken);
        var whilePaused = await ListAsync(customer.AccessToken);
        await _client.SendAsAsync(HttpMethod.Post, $"{ProviderApi.Provider}/publish", provider.AccessToken);
        var afterPublishing = await ListAsync(customer.AccessToken);

        Assert.Empty(whilePaused.Items);
        Assert.Equal(provider.Id, Assert.Single(afterPublishing.Items).Id);
    }

    [Fact]
    public async Task PublicProfile_TellsWhetherTheCallerSavedIt()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);
        var other = await factory.SignInNewAccountAsync(_client);
        await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);
        var profile = $"/api/v1/providers/{provider.Id}";

        var withToken = await (await _client.SendAsAsync(HttpMethod.Get, profile, customer.AccessToken)).Content.ReadApiAsync<ProviderPublicResponse>();
        var otherAccount = await (await _client.SendAsAsync(HttpMethod.Get, profile, other.AccessToken)).Content.ReadApiAsync<ProviderPublicResponse>();
        var anonymous = await (await _client.GetAsync(profile, TestContext.Current.CancellationToken)).Content.ReadApiAsync<ProviderPublicResponse>();

        Assert.True(withToken.IsFavorite);
        Assert.False(otherAccount.IsFavorite);
        Assert.False(anonymous.IsFavorite);
    }

    [Fact]
    public async Task DeleteAccount_RemovesTheFavoritesOfTheAccount()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);
        await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);

        await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", customer.AccessToken);

        Assert.Equal(0, await factory.QueryAsync(db => db.Favorites.CountAsync(
            favorite => favorite.UserId == customer.User.Id,
            TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Favorites_WithoutToken_ReturnUnauthorized()
    {
        var anonymous = factory.CreateAuthClient();
        var id = Guid.NewGuid();

        var list = await anonymous.GetAsync(Favorites, TestContext.Current.CancellationToken);
        var save = await anonymous.PutAsync(Favorite(id), null, TestContext.Current.CancellationToken);
        var remove = await anonymous.DeleteAsync(Favorite(id), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, list.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, save.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, remove.StatusCode);
    }

    [Fact]
    public async Task Favorites_OfAnotherAccount_NeverAppearInTheList()
    {
        var provider = await PublishAsync();
        var owner = await factory.SignInNewAccountAsync(_client);
        var other = await factory.SignInNewAccountAsync(_client);
        await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), owner.AccessToken);

        var list = await ListAsync(other.AccessToken);

        Assert.Empty(list.Items);
    }

    [Fact]
    public async Task Favorites_NeverExposePhoneAddressOrCoordinates()
    {
        var provider = await PublishAsync();
        var customer = await factory.SignInNewAccountAsync(_client);
        await _client.SendAsAsync(HttpMethod.Put, Favorite(provider.Id), customer.AccessToken);

        var json = await (await _client.SendAsAsync(HttpMethod.Get, Favorites, customer.AccessToken)).Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Carlos Menezes", json, StringComparison.Ordinal);
        Assert.DoesNotContain("991234567", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Rua de Teste", json, StringComparison.Ordinal);
        Assert.DoesNotContain("atitude", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ongitude", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Favorites_BeyondTheTenthPage_ReturnsBadRequest()
    {
        var customer = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendAsAsync(HttpMethod.Get, $"{Favorites}?page=11", customer.AccessToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string Favorite(Guid providerId) => $"{Favorites}/{providerId}";

    private Task<PublishedProvider> PublishAsync() => factory.PublishAsync(_client, new SearchableProvider(SearchApi.NextSpot()));

    private async Task<FavoritesPageResponse> ListAsync(string accessToken)
    {
        var response = await _client.SendAsAsync(HttpMethod.Get, Favorites, accessToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadApiAsync<FavoritesPageResponse>();
    }
}
