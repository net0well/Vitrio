using Vitrio.Domain.Orders;
using static Vitrio.Domain.Tests.Orders.OrderTestData;

namespace Vitrio.Domain.Tests.Orders;

public sealed class QuoteTests
{
    [Fact]
    public void SendQuote_ForThreeDaysAhead_ValidFor48HoursAndCountsAsFirstResponse()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);
        clock.Advance(TimeSpan.FromMinutes(20));

        var result = order.SendQuote(Terms(Today(clock).AddDays(3), "14:00", 120), clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Quoted, order.Status);
        Assert.Equal(clock.UtcNow.AddHours(48), order.QuoteValidUntil);
        Assert.Equal(clock.UtcNow, order.FirstResponseAt);
        Assert.Equal(Local(2026, 11, 9, 14), order.Quote!.Schedule.StartsAt);
        Assert.Equal(Local(2026, 11, 9, 16), order.Quote.Schedule.EndsAt);
        Assert.Equal(18000, order.Quote.Amount.Cents);
        var sent = Assert.IsType<QuoteSent>(order.DomainEvents[^1]);
        Assert.Equal(1, sent.QuoteVersion);
    }

    [Fact]
    public void SendQuote_AgainWhileQuoted_ReplacesItKeepingTheFirstResponse()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Quoted(clock);
        var firstResponse = order.FirstResponseAt;
        clock.Advance(TimeSpan.FromHours(1));

        order.SendQuote(Terms(Today(clock).AddDays(3), amount: 150m), clock);

        Assert.Equal(2, order.Quote!.Version);
        Assert.Equal(15000, order.Quote.Amount.Cents);
        Assert.Equal(clock.UtcNow.AddHours(48), order.QuoteValidUntil);
        Assert.Equal(firstResponse, order.FirstResponseAt);
    }

    [Fact]
    public void SendQuote_At3pmForTomorrowAt8am_IsValidUntil7am()
    {
        var clock = ClockAt(Local(2026, 11, 6, 15));
        var order = Sent(clock, Today(clock).AddDays(1), PreferredPeriod.Morning);

        order.SendQuote(Terms(Today(clock).AddDays(1), "08:00"), clock);

        Assert.Equal(Local(2026, 11, 7, 7), order.QuoteValidUntil);
    }

    [Fact]
    public void SendQuote_StartingInLessThanAnHour_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 13, 10));
        var order = Sent(clock, Today(clock));

        var result = order.SendQuote(Terms(Today(clock), "14:00"), clock);

        Assert.Equal(OrderErrors.StartsTooSoon, result.Error);
    }

    [Theory]
    [InlineData("14:15", 60)]
    [InlineData("14:00", 45)]
    [InlineData("14:00", 510)]
    public void SendQuote_OutsideTheHalfHourSteps_Fails(string start, int duration)
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);

        var result = order.SendQuote(Terms(Today(clock).AddDays(3), start, duration), clock);

        Assert.Equal(OrderErrors.InvalidSchedule, result.Error);
    }

    [Fact]
    public void SendQuote_PastMidnight_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);

        var result = order.SendQuote(Terms(Today(clock).AddDays(3), "23:30", 60), clock);

        Assert.Equal(OrderErrors.ScheduleCrossesMidnight, result.Error);
    }

    [Fact]
    public void SendQuote_WithMaterialDetailsButNoMaterial_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);

        var result = order.SendQuote(Terms(Today(clock).AddDays(3), materialIncluded: false, materialDetails: "disjuntor 40 A"), clock);

        Assert.Equal(OrderErrors.InvalidMaterialDetails, result.Error);
    }

    [Theory]
    [InlineData(0.99)]
    [InlineData(100000.01)]
    [InlineData(10.005)]
    public void SendQuote_WithAnAmountOutOfRange_Fails(double amount)
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);

        var result = order.SendQuote(Terms(Today(clock).AddDays(3), amount: (decimal)amount), clock);

        Assert.Equal(OrderErrors.InvalidAmount, result.Error);
    }

    [Fact]
    public void SendQuote_AfterTheResponseDeadline_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);
        clock.Set(order.RespondBy);

        var result = order.SendQuote(Terms(Today(clock).AddDays(3)), clock);

        Assert.Equal(OrderErrors.ResponseWindowClosed, result.Error);
    }

    [Fact]
    public void SendQuote_WhenScheduled_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock);

        var result = order.SendQuote(Terms(Today(clock).AddDays(4)), clock);

        Assert.Equal(OrderErrors.CannotQuote, result.Error);
    }
}
