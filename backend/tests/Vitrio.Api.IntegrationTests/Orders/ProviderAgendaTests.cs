using System.Globalization;
using System.Net;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Common;
using Vitrio.Domain.Orders;
using static Vitrio.Api.IntegrationTests.Orders.OrderApi;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed class ProviderAgendaTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task Agenda_MarksScheduledQuotedAndDoneDays_WithoutExpiredQuotesOrCancellations()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var today = LocalToday(now);
        var threeDaysAgo = factory.ClockAgo(TimeSpan.FromDays(3));
        var scheduled = await factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(today.AddDays(1)), now).ShouldSucceed();
            order.AcceptQuote(1, now).ShouldSucceed();
        });
        var quoted = await factory.SeedOrderAsync(scenario, order => order.SendQuote(Terms(today.AddDays(2), startHour: 9, durationMinutes: 120), now).ShouldSucceed());
        await factory.SeedOrderAsync(
            scenario,
            order => order.SendQuote(Terms(LocalToday(threeDaysAgo).AddDays(4)), threeDaysAgo).ShouldSucceed(),
            createdAgo: TimeSpan.FromDays(3));
        var done = await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(Terms(LocalToday(threeDaysAgo).AddDays(2), startHour: 10), threeDaysAgo).ShouldSucceed();
                order.AcceptQuote(1, threeDaysAgo).ShouldSucceed();
                order.Complete(OrderParty.Provider, factory.ClockAgo(TimeSpan.FromHours(1))).ShouldSucceed();
            },
            createdAgo: TimeSpan.FromDays(3));
        await factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(today.AddDays(1), startHour: 16), now).ShouldSucceed();
            order.AcceptQuote(1, now).ShouldSucceed();
            order.Cancel(OrderParty.Customer, CancellationReason.TimeNoLongerWorks, null, now).ShouldSucceed();
        });

        var items = await AgendaAsync(scenario.Provider.AccessToken, today.AddDays(-7), today.AddDays(7));

        Assert.Equal(
            [
                (done, AgendaItemKind.Done, today.AddDays(-1), "10:00", "11:00"),
                (scheduled, AgendaItemKind.Scheduled, today.AddDays(1), "14:00", "15:00"),
                (quoted, AgendaItemKind.Quoted, today.AddDays(2), "09:00", "11:00")
            ],
            items.Select(item => (item.OrderId, item.Kind, item.Date, item.Start, item.End)));
        Assert.Equal("Bruno Costa", items[1].CustomerName);
        Assert.Equal("Bruno C.", items[2].CustomerName);
        Assert.NotNull(items[2].QuoteValidUntil);
    }

    [Fact]
    public async Task Agenda_LongerThanFortyTwoDays_ReturnsBadRequest()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var today = LocalToday(factory.ClockAgo(TimeSpan.Zero));

        var response = await _client.SendAsAsync(
            HttpMethod.Get,
            $"/api/v1/provider/agenda?from={Iso(today)}&to={Iso(today.AddDays(42))}",
            scenario.Provider.AccessToken);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
    }

    [Fact]
    public async Task Dashboard_CountsNewOrdersAndServicesDoneThisMonth()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero).UtcNow;
        var threeDaysAgo = factory.ClockAgo(TimeSpan.FromDays(3));
        for (var index = 0; index < 2; index++)
        {
            await factory.SeedOrderAsync(
                scenario,
                order =>
                {
                    order.SendQuote(Terms(LocalToday(threeDaysAgo).AddDays(1), startHour: 10), threeDaysAgo).ShouldSucceed();
                    order.AcceptQuote(1, threeDaysAgo).ShouldSucceed();
                    order.Complete(OrderParty.Customer, factory.ClockAgo(TimeSpan.Zero)).ShouldSucceed();
                },
                createdAgo: TimeSpan.FromDays(3));
        }

        var monthStart = CityTime.MonthOf(now, CityTime.Zone(OrderApi.TimeZone)).Start;
        var lastMonthAgo = now - monthStart + TimeSpan.FromDays(5);
        var lastMonthCreation = factory.ClockAgo(lastMonthAgo);
        await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(Terms(LocalToday(lastMonthCreation).AddDays(1), startHour: 10), lastMonthCreation).ShouldSucceed();
                order.AcceptQuote(1, lastMonthCreation).ShouldSucceed();
                order.Complete(OrderParty.Provider, factory.ClockAgo(now - monthStart + TimeSpan.FromHours(1))).ShouldSucceed();
            },
            createdAgo: lastMonthAgo);
        var newest = await factory.SeedOrderAsync(scenario);
        await factory.SeedOrderAsync(scenario, createdAgo: TimeSpan.FromMinutes(5));

        var dashboard = await (await _client.SendAsAsync(HttpMethod.Get, "/api/v1/provider/dashboard", scenario.Provider.AccessToken))
            .Content.ReadApiAsync<ProviderDashboardResponse>();

        Assert.Equal((2, 2), (dashboard.NewOrders, dashboard.ServicesThisMonth));
        Assert.Equal(2, dashboard.LatestNewOrders.Count);
        Assert.Equal(newest, dashboard.LatestNewOrders[0].Id);
    }

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private async Task<List<AgendaItemResponse>> AgendaAsync(string accessToken, DateOnly from, DateOnly to)
    {
        var response = await _client.SendAsAsync(HttpMethod.Get, $"/api/v1/provider/agenda?from={Iso(from)}&to={Iso(to)}", accessToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadApiAsync<List<AgendaItemResponse>>();
    }
}
