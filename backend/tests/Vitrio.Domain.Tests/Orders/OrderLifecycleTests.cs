using Vitrio.Domain.Orders;
using static Vitrio.Domain.Tests.Orders.OrderTestData;

namespace Vitrio.Domain.Tests.Orders;

public sealed class OrderLifecycleTests
{
    [Fact]
    public void Expire_ASentOrderPastItsDeadline_ExpiresIt()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);
        clock.Set(order.RespondBy.AddMinutes(-1));
        Assert.False(order.Expire(clock));

        clock.Set(order.RespondBy);

        Assert.True(order.Expire(clock));
        Assert.Equal(OrderStatus.Expired, order.Status);
        Assert.IsType<OrderExpired>(order.DomainEvents[^1]);
        Assert.False(order.Expire(clock));
    }

    [Fact]
    public void Expire_AQuotePastItsValidity_ExpiresTheQuote()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Quoted(clock);
        clock.Set(order.QuoteValidUntil!.Value);

        Assert.True(order.Expire(clock));
        Assert.Equal(OrderStatus.QuoteExpired, order.Status);
        Assert.IsType<QuoteExpired>(order.DomainEvents[^1]);
    }

    [Fact]
    public void Contact_IsVisibleUntilSevenDaysAfterTheServiceEnds()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock, Today(clock).AddDays(1));
        var end = order.Schedule!.EndsAt;
        clock.Set(end.AddDays(3));
        order.Complete(OrderParty.Customer, clock);

        Assert.True(order.IsContactVisibleToProvider(end.AddDays(3)));
        Assert.True(order.IsContactVisibleToProvider(end.AddDays(7).AddMinutes(-1)));
        Assert.False(order.IsContactVisibleToProvider(end.AddDays(8)));
    }

    [Fact]
    public void Contact_IsHiddenBeforeTheQuoteIsAccepted()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Quoted(clock);

        Assert.False(order.IsContactVisibleToProvider(clock.UtcNow));
        Assert.Null(order.ContactVisibleUntil);
    }

    [Fact]
    public void ErasePersonalData_NinetyDaysAfterTheService_KeepsOnlyTheNeighborhoodAndShortName()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(1), photos: 2), clock).Value;
        order.CompletePhoto(order.Photos[0].Id, "orders/foto-1.webp");
        order.SendQuote(Terms(Today(clock).AddDays(1)), clock);
        order.AcceptQuote(1, clock);
        clock.Set(order.Schedule!.EndsAt);
        order.Complete(OrderParty.Customer, clock);
        clock.Set(order.Schedule.EndsAt.AddDays(89));
        Assert.Empty(order.ErasePersonalData(clock));

        clock.Set(order.Schedule.EndsAt.AddDays(91));
        var removed = order.ErasePersonalData(clock);

        Assert.Equal(["orders/foto-1.webp"], removed);
        Assert.Null(order.Address);
        Assert.Null(order.Contact);
        Assert.Equal(clock.UtcNow, order.PersonalDataErasedAt);
        Assert.Equal("Mariana S.", order.CustomerDisplayName);
        Assert.Equal("Centro", order.Place.Neighborhood);
        Assert.All(order.Photos, photo => Assert.Equal(OrderPhotoStatus.Removed, photo.Status));
        Assert.False(order.IsContactVisibleToProvider(clock.UtcNow));
    }

    [Fact]
    public void ErasePersonalData_OfADeclinedOrder_CountsFromWhenItClosed()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);
        order.Decline(DeclineReason.NoAvailability, null, clock);

        clock.Advance(TimeSpan.FromDays(91));

        Assert.True(order.CanErasePersonalData(clock.UtcNow));
    }

    [Fact]
    public void ErasePersonalData_OfAnOpenOrder_DoesNothing()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);
        clock.Advance(TimeSpan.FromDays(200));

        Assert.False(order.CanErasePersonalData(clock.UtcNow));
    }

    [Fact]
    public void AllowedActions_FollowTheStateAndTheClock()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var sent = Sent(clock);
        var quoted = Quoted(clock);
        var scheduled = Scheduled(clock, Today(clock).AddDays(1));

        Assert.Equal([OrderAction.SendQuote, OrderAction.Decline], sent.AllowedActionsFor(OrderParty.Provider, clock.UtcNow));
        Assert.Equal([OrderAction.Cancel], sent.AllowedActionsFor(OrderParty.Customer, clock.UtcNow));
        Assert.Equal([OrderAction.AcceptQuote, OrderAction.DeclineQuote, OrderAction.Cancel], quoted.AllowedActionsFor(OrderParty.Customer, clock.UtcNow));
        Assert.Equal([OrderAction.ReplaceQuote], quoted.AllowedActionsFor(OrderParty.Provider, clock.UtcNow));
        Assert.Equal([OrderAction.Reschedule, OrderAction.Cancel], scheduled.AllowedActionsFor(OrderParty.Provider, clock.UtcNow));
        Assert.Equal([OrderAction.Cancel, OrderAction.Complete], scheduled.AllowedActionsFor(OrderParty.Customer, scheduled.Schedule!.StartsAt));
        Assert.Empty(sent.AllowedActionsFor(OrderParty.Provider, sent.RespondBy));
    }
}
