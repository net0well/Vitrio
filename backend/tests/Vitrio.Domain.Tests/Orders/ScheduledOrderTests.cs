using Vitrio.Domain.Orders;
using static Vitrio.Domain.Tests.Orders.OrderTestData;

namespace Vitrio.Domain.Tests.Orders;

public sealed class ScheduledOrderTests
{
    [Fact]
    public void Reschedule_BeforeTheStart_MovesTheScheduleAndTheContactWindow()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock, Today(clock).AddDays(1));

        var result = order.Reschedule(Today(clock).AddDays(2), new TimeOnly(9, 0), 60, clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Scheduled, order.Status);
        Assert.Equal(Local(2026, 11, 8, 9), order.Schedule!.StartsAt);
        Assert.Equal(1, order.RescheduleCount);
        Assert.Equal(Local(2026, 11, 8, 10).AddDays(7), order.ContactVisibleUntil);
        Assert.Equal(1, Assert.IsType<OrderRescheduled>(Assert.Single(order.DomainEvents)).RescheduleCount);
    }

    [Fact]
    public void Reschedule_AThirdTime_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock, Today(clock).AddDays(1));
        order.Reschedule(Today(clock).AddDays(2), new TimeOnly(9, 0), 60, clock);
        order.Reschedule(Today(clock).AddDays(3), new TimeOnly(9, 0), 60, clock);

        var result = order.Reschedule(Today(clock).AddDays(4), new TimeOnly(9, 0), 60, clock);

        Assert.Equal(OrderErrors.RescheduleLimitReached, result.Error);
    }

    [Fact]
    public void Reschedule_AfterTheStart_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock, Today(clock).AddDays(1));
        clock.Set(Local(2026, 11, 7, 14, 30));

        var result = order.Reschedule(Today(clock).AddDays(1), new TimeOnly(9, 0), 60, clock);

        Assert.Equal(OrderErrors.CannotReschedule, result.Error);
    }

    [Fact]
    public void Reschedule_ToLessThanAnHourFromNow_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10, 20));
        var order = Scheduled(clock, Today(clock).AddDays(1));

        var result = order.Reschedule(Today(clock), new TimeOnly(11, 0), 60, clock);

        Assert.Equal(OrderErrors.StartsTooSoon, result.Error);
    }

    [Fact]
    public void Cancel_ByTheCustomerWithAReason_ClosesTheOrderAndHidesTheContact()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock);

        var result = order.Cancel(OrderParty.Customer, CancellationReason.TimeNoLongerWorks, null, clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.False(order.IsContactVisibleToProvider(clock.UtcNow));
        var cancelled = Assert.IsType<OrderCancelled>(Assert.Single(order.DomainEvents));
        Assert.Equal((OrderParty.Customer, CancellationReason.TimeNoLongerWorks, false), (cancelled.CancelledBy, cancelled.ReasonCode, cancelled.WithinTwoHours));
    }

    [Fact]
    public void Cancel_ByTheCustomerWithAProviderReason_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock);

        var result = order.Cancel(OrderParty.Customer, CancellationReason.Unforeseen, null, clock);

        Assert.Equal(OrderErrors.InvalidCancellationReason, result.Error);
    }

    [Fact]
    public void Cancel_WithOtherAndNoText_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock);

        var result = order.Cancel(OrderParty.Provider, CancellationReason.Other, "", clock);

        Assert.Equal(OrderErrors.CancellationDetailsRequired, result.Error);
    }

    [Fact]
    public void Cancel_ByTheProviderAnHourBeforeTheStart_IsLate()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock, Today(clock).AddDays(1));
        clock.Set(Local(2026, 11, 7, 13));

        order.Cancel(OrderParty.Provider, CancellationReason.Unforeseen, null, clock);

        Assert.True(Assert.IsType<OrderCancelled>(Assert.Single(order.DomainEvents)).WithinTwoHours);
    }

    [Fact]
    public void Cancel_ByTheProviderBeforeScheduling_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Quoted(clock);

        var result = order.Cancel(OrderParty.Provider, CancellationReason.Unforeseen, null, clock);

        Assert.Equal(OrderErrors.CannotCancel, result.Error);
    }

    [Fact]
    public void Cancel_BySystemWhenTheAccountIsDeleted_ClosesAnOpenOrder()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Sent(clock);

        var result = order.Cancel(OrderParty.System, CancellationReason.AccountDeleted, null, clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderParty.System, order.CancelledBy);
    }

    [Fact]
    public void Complete_BeforeTheStart_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock, Today(clock).AddDays(1));

        var result = order.Complete(OrderParty.Customer, clock);

        Assert.Equal(OrderErrors.CompleteBeforeStart, result.Error);
    }

    [Fact]
    public void Complete_AfterTheStart_RecordsWhoCompleted()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock, Today(clock).AddDays(1));
        clock.Set(Local(2026, 11, 7, 14));

        order.Complete(OrderParty.Provider, clock);

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(OrderParty.Provider, order.CompletedBy);
        Assert.Equal(OrderParty.Provider, Assert.IsType<OrderCompleted>(Assert.Single(order.DomainEvents)).CompletedBy);
    }

    [Fact]
    public void CompleteForgotten_SevenDaysAfterTheEnd_CompletesBySystem()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));
        var order = Scheduled(clock, Today(clock).AddDays(1));
        clock.Set(Local(2026, 11, 7, 15).AddDays(7).AddMinutes(-1));
        Assert.False(order.CompleteForgotten(clock));

        clock.Advance(TimeSpan.FromMinutes(1));

        Assert.True(order.CompleteForgotten(clock));
        Assert.Equal(OrderParty.System, order.CompletedBy);
    }
}
