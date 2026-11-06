using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Search;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Common;
using Vitrio.Domain.Orders;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Api.IntegrationTests.Orders;

public static class OrderApi
{
    public const string Route = "/api/v1/orders";
    public const string TimeZone = "America/Sao_Paulo";
    public const string Title = "Chuveiro desarmando o disjuntor";
    public const string Description = "O chuveiro desarma o disjuntor quando ligo no modo inverno.";

    public static IClock ClockAgo(this ApiFactory factory, TimeSpan ago) => new ShiftedClock(factory.Clock, -ago);

    public static DateOnly LocalToday(IClock clock) => CityTime.Today(clock.UtcNow, CityTime.Zone(TimeZone));

    public static DateOnly Tomorrow(this ApiFactory factory) => LocalToday(factory.ClockAgo(TimeSpan.Zero)).AddDays(1);

    public static DateTimeOffset LocalTime(DateOnly date, int hour) =>
        CityTime.ToUtc(date, new TimeOnly(hour, 0), CityTime.Zone(TimeZone));

    public static async Task<OrderScenario> StartOrderScenarioAsync(this ApiFactory factory, HttpClient client, int categoryId = 1)
    {
        var spot = SearchApi.NextSpot();
        var provider = await factory.PublishAsync(client, new SearchableProvider(spot) { CategoryIds = [categoryId] });
        var customer = await factory.SignInCustomerAsync(client, spot.Offset(1));
        return new OrderScenario(provider, customer, spot);
    }

    public static async Task<OrderCustomer> SignInCustomerAsync(this ApiFactory factory, HttpClient client, GeoSpot point, bool withPhone = true)
    {
        var auth = await factory.SignInNewAccountAsync(client);
        return await client.PrepareCustomerAsync(factory, auth.AccessToken, auth.User.Id, point, withPhone);
    }

    public static async Task<OrderCustomer> PrepareCustomerAsync(
        this HttpClient client,
        ApiFactory factory,
        string accessToken,
        Guid userId,
        GeoSpot point,
        bool withPhone = true)
    {
        if (withPhone)
        {
            await client.SendOkAsync(HttpMethod.Patch, "/api/v1/me", accessToken, new { phoneNumber = "16988887777" });
        }

        var city = SearchableProvider.FarAway;
        var cep = factory.Ceps.Register(city.IbgeCode, city.Name, city.State, point.Latitude, point.Longitude);
        var added = await client.SendJsonAsAsync(HttpMethod.Post, "/api/v1/me/addresses", accessToken, new { label = "Casa", cep, number = "45" });
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        var address = await added.Content.ReadApiAsync<AddressResponse>();
        return new OrderCustomer(accessToken, userId, address.Id, point);
    }

    public static object NewOrder(
        Guid providerId,
        Guid addressId,
        DateOnly day,
        IReadOnlyList<string>? photoKeys = null,
        Guid? serviceId = null,
        string period = "afternoon",
        string title = Title) =>
        new
        {
            providerId,
            serviceId,
            title,
            description = Description,
            preferredDay = day,
            preferredPeriod = period,
            addressId,
            photoKeys = photoKeys ?? []
        };

    public static Task<HttpResponseMessage> CreateOrderAsync(this HttpClient client, string accessToken, object body, Guid? idempotencyKey = null, bool withKey = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Route) { Content = JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (withKey)
        {
            request.Headers.Add("Idempotency-Key", (idempotencyKey ?? Guid.NewGuid()).ToString());
        }

        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public static async Task<OrderCustomerView> CreateOrderOkAsync(this HttpClient client, OrderScenario scenario, DateOnly day, IReadOnlyList<string>? photoKeys = null)
    {
        var response = await client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, day, photoKeys));
        Assert.True(response.StatusCode == HttpStatusCode.Created, $"{(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadApiAsync<OrderCustomerView>();
    }

    public static async Task<OrderCustomerView> GetOrderAsync(this HttpClient client, string accessToken, Guid orderId)
    {
        var response = await client.SendAsAsync(HttpMethod.Get, $"{Route}/{orderId}", accessToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadApiAsync<OrderCustomerView>();
    }

    public static QuoteTerms Terms(DateOnly date, int startHour = 14, int durationMinutes = 60) =>
        new(QuoteType.FixedPrice, 180m, date, new TimeOnly(startHour, 0), durationMinutes, true, "disjuntor 40 A", "Troco o disjuntor e reviso a fiação.");

    public static async Task<Guid> SeedOrderAsync(
        this ApiFactory factory,
        OrderScenario scenario,
        Action<Order>? change = null,
        TimeSpan? createdAgo = null,
        IReadOnlyList<string>? uploadKeys = null,
        int categoryId = 1)
    {
        var clock = factory.ClockAgo(createdAgo ?? TimeSpan.Zero);
        var city = SearchableProvider.FarAway;
        var request = new OrderRequest(
            null,
            "Outro",
            categoryId,
            Title,
            Description,
            LocalToday(clock).AddDays(1),
            PreferredPeriod.Afternoon,
            new OrderAddress("Rua de Teste", "45", null, "60000001"),
            new OrderContact("Bruno Costa", "16988887777"),
            new OrderPlace(city.IbgeCode, city.Name, city.State, "Bairro de Teste", TimeZone, scenario.Customer.Point.Latitude, scenario.Customer.Point.Longitude, false),
            uploadKeys ?? []);
        var order = Order.Create(scenario.Customer.Id, scenario.Provider.Id, request, clock).Value;
        change?.Invoke(order);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order.Id;
    }

    public static void ShouldSucceed(this Result result) =>
        Assert.True(result.IsSuccess, result.Error?.Code);

    public static string Hour(int hour) => string.Create(CultureInfo.InvariantCulture, $"{hour:00}:00");

    private static async Task SendOkAsync(this HttpClient client, HttpMethod method, string url, string accessToken, object body)
    {
        var response = await client.SendJsonAsAsync(method, url, accessToken, body);
        Assert.True(response.IsSuccessStatusCode, $"{method} {url}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }
}
