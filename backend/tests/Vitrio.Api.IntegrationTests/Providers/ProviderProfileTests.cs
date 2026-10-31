using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Api.IntegrationTests.Providers;

public sealed class ProviderProfileTests(ApiFactory factory)
{
    private const string Provider = ProviderApi.Provider;

    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task CreateProvider_ForAccountWithoutProfile_StartsAsDraftAndGrantsTheProviderRole()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var created = await _client.SendJsonAsAsync(HttpMethod.Post, Provider, auth.AccessToken, new { displayName = "Carlos Eletricista", headline = "Eletricista" });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var draft = await created.Content.ReadApiAsync<ProviderPrivateResponse>();
        Assert.Equal(ProviderStatus.Draft, draft!.Status);
        Assert.Equal("Carlos Eletricista", draft.DisplayName);
        Assert.Contains(PublishChecklist.About, draft.Missing);

        var tokens = await (await _client.RefreshAsync(auth.RefreshToken)).ReadTokensAsync();
        var me = await (await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me", tokens.AccessToken))
            .Content.ReadFromJsonAsync<MeResponse>(TestContext.Current.CancellationToken);
        Assert.True(me!.HasProviderProfile);
        Assert.Contains("provider", me.Roles);
        Assert.Equal(HttpStatusCode.OK, (await _client.SendAsAsync(HttpMethod.Get, Provider, tokens.AccessToken)).StatusCode);
    }

    [Fact]
    public async Task CreateProvider_Twice_ReturnsConflict()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, Provider, accessToken, new { displayName = "Outro Nome", headline = "Pintor" });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "provider.already_exists");
    }

    [Fact]
    public async Task CreateProvider_WithAccountPhone_PrefillsTheContactPhone()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        await _client.SendJsonAsAsync(HttpMethod.Patch, "/api/v1/me", auth.AccessToken, new { phoneNumber = "(16) 99876-5432" });

        var created = await _client.SendJsonAsAsync(HttpMethod.Post, Provider, auth.AccessToken, new { displayName = "Ana Pintora", headline = "Pintora" });

        var draft = await created.Content.ReadApiAsync<ProviderPrivateResponse>();
        Assert.Equal("+5516998765432", draft!.ContactPhone);
        Assert.DoesNotContain(PublishChecklist.ContactPhone, draft.Missing);
    }

    [Fact]
    public async Task ProviderRoutes_WithoutToken_ReturnUnauthorized()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PostAsJsonAsync(Provider, new { displayName = "Carlos", headline = "Eletricista" }, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync(Provider, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync($"{Provider}/dashboard", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task ProviderRoutes_ForAccountWithoutProfile_AreForbidden()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendAsAsync(HttpMethod.Get, Provider, auth.AccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Publish_WithoutAboutAndServices_ListsWhatIsMissing()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendAsAsync(HttpMethod.Post, $"{Provider}/publish", accessToken);

        var problem = await response.AssertProblemAsync(HttpStatusCode.UnprocessableEntity, "provider.incomplete");
        var missing = problem.GetProperty("missing").EnumerateArray().Select(item => item.GetString()).ToList();
        Assert.Contains("about", missing);
        Assert.Contains("services", missing);
    }

    [Fact]
    public async Task Publish_WhenComplete_PublishesAndPauseThenPublishAgainWorks()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        await _client.CompleteAsync(accessToken);

        var published = await _client.SendAsAsync(HttpMethod.Post, $"{Provider}/publish", accessToken);
        var paused = await _client.SendAsAsync(HttpMethod.Post, $"{Provider}/pause", accessToken);
        var republished = await _client.SendAsAsync(HttpMethod.Post, $"{Provider}/publish", accessToken);

        Assert.Equal(HttpStatusCode.OK, published.StatusCode);
        Assert.Equal("paused", (await paused.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("status").GetString());
        Assert.Equal("published", (await republished.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("status").GetString());
        var provider = await _client.GetProviderAsync(accessToken);
        Assert.Equal(ProviderStatus.Published, provider.Status);
        Assert.Empty(provider.Missing);
    }

    [Fact]
    public async Task UpdateProvider_WithPhoneInAbout_ExplainsThatContactIsThroughWhatsApp()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendJsonAsAsync(
            HttpMethod.Patch,
            Provider,
            accessToken,
            new { about = "Me chame no zap 16 99123-4567 para um orçamento rápido e sem compromisso." });

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.Contains("WhatsApp", problem.GetProperty("errors").GetProperty("about")[0].GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CompleteProfile_ShowsEverythingBackToTheOwner()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        await _client.CompleteAsync(accessToken);
        await _client.SendJsonAsAsync(
            HttpMethod.Put,
            $"{Provider}/availability",
            accessToken,
            new { slots = new[] { new { dayOfWeek = "monday", start = "08:00", end = "12:00" }, new { dayOfWeek = "monday", start = "18:00", end = "24:00" } }, handlesEmergencies = true });

        var provider = await _client.GetProviderAsync(accessToken);

        Assert.Equal("+5516991234567", provider.ContactPhone);
        Assert.True(provider.WhatsAppEnabled);
        Assert.True(provider.HandlesEmergencies);
        Assert.Equal(["Elétrica", "Marido de aluguel"], provider.Categories.Select(category => category.Name));
        Assert.True(provider.Categories[0].IsPrimary);
        Assert.Equal("Rua Rui Barbosa", provider.ServiceArea!.Street);
        Assert.Equal("Jaboticabal", provider.ServiceArea.City.Name);
        Assert.Equal(15, provider.ServiceArea.RadiusKm);
        Assert.False(provider.ServiceArea.LocationApproximate);
        Assert.Equal(["Centro", "Jardim Nova Aparecida"], provider.ServiceArea.Neighborhoods);
        Assert.Equal(80m, Assert.Single(provider.Services).PriceFrom);
        Assert.Equal(["08:00", "18:00"], provider.Availability.Select(slot => slot.Start));
        Assert.Equal("24:00", provider.Availability[1].End);
    }

    [Fact]
    public async Task SetServiceArea_KeepsTheBaseAddressEncryptedAndOnlyThePublicCellInClear()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client, "Paula Encanadora");
        await _client.CompleteAsync(accessToken);

        var row = await factory.QueryAsync(db => db.Database
            .SqlQueryRaw<StoredArea>(
                "select base_street, base_location, public_latitude from providers p join users u on u.id = p.user_id where p.display_name = 'Paula Encanadora'")
            .SingleAsync(TestContext.Current.CancellationToken));

        Assert.DoesNotContain("Rui Barbosa", row.base_street, StringComparison.Ordinal);
        Assert.DoesNotContain("-21.2547", row.base_location, StringComparison.Ordinal);
        Assert.NotEqual(-21.2547, row.public_latitude);
    }

    [Fact]
    public async Task SetServiceArea_WithCepWithoutPoint_UsesTheCityAndMarksTheAreaAsApproximate()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendJsonAsAsync(
            HttpMethod.Put,
            $"{Provider}/service-area",
            accessToken,
            new { cep = FakeCepLookup.JaboticabalWithoutPoint, number = "45", radiusKm = 10, neighborhoods = Array.Empty<string>() });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await response.Content.ReadFromJsonAsync<ServiceAreaSavedResponse>(TestContext.Current.CancellationToken);
        Assert.True(saved!.LocationApproximate);
        Assert.Equal("Jaboticabal", saved.City.Name);
    }

    [Theory]
    [InlineData(FakeCepLookup.Unknown, HttpStatusCode.BadRequest, "address.cep_not_found")]
    [InlineData(FakeCepLookup.Unreachable, HttpStatusCode.ServiceUnavailable, "address.cep_unavailable")]
    public async Task SetServiceArea_WithUnknownOrUnreachableCep_ReturnsTheProblem(string cep, HttpStatusCode status, string code)
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendJsonAsAsync(
            HttpMethod.Put,
            $"{Provider}/service-area",
            accessToken,
            new { cep, number = "45", radiusKm = 10, neighborhoods = Array.Empty<string>() });

        await response.AssertProblemAsync(status, code);
    }

    [Fact]
    public async Task SetCategories_WithUnknownCategory_ReturnsBadRequest()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Put, $"{Provider}/categories", accessToken, new { categoryIds = new[] { 1, 999 }, primaryCategoryId = 1 });

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "provider.unknown_category");
    }

    [Fact]
    public async Task SetAvailability_WithOverlappingSlots_ReturnsBadRequest()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendJsonAsAsync(
            HttpMethod.Put,
            $"{Provider}/availability",
            accessToken,
            new { slots = new[] { new { dayOfWeek = "monday", start = "08:00", end = "12:00" }, new { dayOfWeek = "monday", start = "11:30", end = "14:00" } } });

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "provider.invalid_availability");
    }

    [Fact]
    public async Task SetAvailableToday_OnADraft_ReturnsConflict()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Put, $"{Provider}/available-today", accessToken, new { value = true });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "provider.not_published");
    }

    [Fact]
    public async Task Dashboard_ShowsProgressAndTheNextStep()
    {
        var accessToken = await factory.SignInAsProviderAsync(_client);
        await _client.CompleteAsync(accessToken);

        var dashboard = await (await _client.SendAsAsync(HttpMethod.Get, $"{Provider}/dashboard", accessToken))
            .Content.ReadApiAsync<ProviderDashboardResponse>();

        Assert.Equal(ProviderStatus.Draft, dashboard!.Status);
        Assert.Equal(PublishChecklist.Publish, dashboard.NextStep);
        Assert.Equal(83, dashboard.ProfileCompleteness);
    }

    [Fact]
    public async Task ListCategories_ReturnsTheFifteenCategoriesWithPublicCache()
    {
        var response = await _client.GetAsync("/api/v1/categories", TestContext.Current.CancellationToken);

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>(TestContext.Current.CancellationToken);
        Assert.Equal(15, categories!.Count);
        Assert.Equal("eletrica", categories[0].Slug);
        Assert.Equal(("eletricista", "eletricistas"), (categories[0].ProfessionalSingular, categories[0].ProfessionalPlural));
        Assert.All(categories, category => Assert.False(string.IsNullOrWhiteSpace(category.ProfessionalPlural)));
        Assert.Contains("public", response.Headers.CacheControl!.ToString(), StringComparison.Ordinal);
    }

    private sealed record StoredArea(string base_street, string base_location, double public_latitude);
}
