using Vitrio.Domain.Orders;
using static Vitrio.Domain.Tests.Orders.OrderTestData;

namespace Vitrio.Domain.Tests.Orders;

public sealed class OrderAnswerTests
{
    [Fact]
    public void AcceptQuote_WithinTheValidity_SchedulesAndReleasesTheAddress()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Quoted(clock);
        order.ClearDomainEvents();
        clock.Advance(TimeSpan.FromHours(2));

        var result = order.AcceptQuote(1, clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Scheduled, order.Status);
        Assert.Equal(order.Quote!.Schedule, order.Schedule);
        Assert.Equal(clock.UtcNow, order.AddressReleasedAt);
        Assert.Collection(
            order.DomainEvents,
            first => Assert.Equal(1, Assert.IsType<QuoteAccepted>(first).QuoteVersion),
            second => Assert.IsType<AddressReleased>(second));
    }

    [Fact]
    public void AcceptQuote_WithAnOldVersion_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Quoted(clock);
        order.SendQuote(Terms(Today(clock).AddDays(3), amount: 200m), clock);

        var result = order.AcceptQuote(1, clock);

        Assert.Equal(OrderErrors.QuoteVersionMismatch, result.Error);
    }

    [Fact]
    public void AcceptQuote_OneMinuteAfterTheValidity_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 15));
        var order = Sent(clock, Today(clock).AddDays(1), PreferredPeriod.Morning);
        order.SendQuote(Terms(Today(clock).AddDays(1), "08:00"), clock);
        clock.Set(Local(2026, 11, 7, 7, 1));

        var result = order.AcceptQuote(1, clock);

        Assert.Equal(OrderErrors.QuoteExpired, result.Error);
    }

    [Fact]
    public void DeclineQuote_ClosesTheOrder()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Quoted(clock);

        order.DeclineQuote(clock);

        Assert.Equal(OrderStatus.QuoteDeclined, order.Status);
        Assert.Equal(clock.UtcNow, order.ClosedAt);
        Assert.IsType<QuoteDeclined>(order.DomainEvents[^1]);
    }

    [Fact]
    public void Decline_WithOtherAndNoText_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);

        var result = order.Decline(DeclineReason.Other, "  ", clock);

        Assert.Equal(OrderErrors.DeclineDetailsRequired, result.Error);
    }

    [Fact]
    public void Decline_WithOtherAndAPhoneNumber_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);

        var result = order.Decline(DeclineReason.Other, "Me chama no 16 99123-4567", clock);

        Assert.Equal(OrderErrors.ContactInDetails, result.Error);
    }

    [Fact]
    public void Decline_OutsideTheArea_ClosesWithTheReasonAndCountsAsResponse()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);

        var result = order.Decline(DeclineReason.OutsideArea, "ignorado", clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Declined, order.Status);
        Assert.Equal(DeclineReason.OutsideArea, order.DeclineReason);
        Assert.Null(order.DeclineDetails);
        Assert.Equal(clock.UtcNow, order.FirstResponseAt);
        Assert.Equal(DeclineReason.OutsideArea, Assert.IsType<OrderDeclined>(order.DomainEvents[^1]).ReasonCode);
    }

    [Fact]
    public void Decline_AfterQuoting_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Quoted(clock);

        var result = order.Decline(DeclineReason.NoAvailability, null, clock);

        Assert.Equal(OrderErrors.CannotDecline, result.Error);
    }
}
