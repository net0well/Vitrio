using System.Net;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Providers.Common;

namespace Vitrio.Api.IntegrationTests.Providers;

public static class ProviderApi
{
    public const string Provider = "/api/v1/provider";

    public const string About = "Eletricista há 12 anos, faço instalações e reparos residenciais com garantia.";

    public static async Task<string> SignInAsProviderAsync(this ApiFactory factory, HttpClient client, string displayName = "Carlos Eletricista")
    {
        var auth = await factory.SignInNewAccountAsync(client);
        var created = await client.SendJsonAsAsync(HttpMethod.Post, Provider, auth.AccessToken, new { displayName, headline = "Eletricista" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        return (await (await client.RefreshAsync(auth.RefreshToken)).ReadTokensAsync()).AccessToken;
    }

    public static async Task CompleteAsync(this HttpClient client, string accessToken, string cep = FakeCepLookup.Jaboticabal)
    {
        await client.SendOkAsync(HttpMethod.Patch, Provider, accessToken, new { about = About, contactPhone = "16991234567", whatsAppEnabled = true });
        await client.SendOkAsync(HttpMethod.Put, $"{Provider}/categories", accessToken, new { categoryIds = new[] { 1, 9 }, primaryCategoryId = 1 });
        await client.SendOkAsync(
            HttpMethod.Put,
            $"{Provider}/service-area",
            accessToken,
            new { cep, number = "120", radiusKm = 15, neighborhoods = new[] { "Centro", "Jardim Nova Aparecida" } });
        await client.SendOkAsync(
            HttpMethod.Put,
            $"{Provider}/services",
            accessToken,
            new { services = new object[] { new { title = "Troca de chuveiro", priceFrom = 80m, priceUnit = "perService" } } });
    }

    public static async Task<ProviderPrivateResponse> GetProviderAsync(this HttpClient client, string accessToken) =>
        (await (await client.SendAsAsync(HttpMethod.Get, Provider, accessToken))
            .Content.ReadApiAsync<ProviderPrivateResponse>());

    private static async Task SendOkAsync(this HttpClient client, HttpMethod method, string url, string accessToken, object body)
    {
        var response = await client.SendJsonAsAsync(method, url, accessToken, body);
        Assert.True(response.IsSuccessStatusCode, $"{method} {url}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }
}
