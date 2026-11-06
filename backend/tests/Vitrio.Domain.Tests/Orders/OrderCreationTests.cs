using Vitrio.Domain.Orders;
using static Vitrio.Domain.Tests.Orders.OrderTestData;

namespace Vitrio.Domain.Tests.Orders;

public sealed class OrderCreationTests
{
    [Fact]
    public void Create_ForThisAfternoonAt4pm_IsDueAt6pmToday()
    {
        var clock = ClockAt(Local(2026, 11, 6, 16));

        var order = Order.Create(CustomerId, ProviderId, Request(Today(clock), PreferredPeriod.Afternoon), clock).Value;

        Assert.Equal(OrderStatus.Sent, order.Status);
        Assert.Equal(Local(2026, 11, 6, 18), order.RespondBy);
        Assert.Equal("Mariana S.", order.CustomerDisplayName);
        var created = Assert.IsType<OrderCreated>(Assert.Single(order.DomainEvents));
        Assert.Equal((order.Id, CustomerId, ProviderId), (created.OrderId, created.CustomerId, created.ProviderId));
    }

    [Fact]
    public void Create_ForThisMorningAfterItEnded_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 13));

        var result = Order.Create(CustomerId, ProviderId, Request(Today(clock), PreferredPeriod.Morning), clock);

        Assert.Equal(OrderErrors.PeriodAlreadyOver, result.Error);
    }

    [Fact]
    public void Create_ForTomorrowMorning_IsDueTomorrowAtNoon()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));

        var order = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(1), PreferredPeriod.Morning), clock).Value;

        Assert.Equal(Local(2026, 11, 7, 12), order.RespondBy);
    }

    [Fact]
    public void Create_ForAFarDay_IsDueIn48Hours()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));

        var order = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(5)), clock).Value;

        Assert.Equal(clock.UtcNow.AddHours(48), order.RespondBy);
    }

    [Theory]
    [InlineData(31)]
    [InlineData(-1)]
    public void Create_OutsideTheNext30Days_Fails(int daysAhead)
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));

        var result = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(daysAhead)), clock);

        Assert.Equal(OrderErrors.InvalidPreferredDay, result.Error);
    }

    [Fact]
    public void Create_ThirtyDaysAhead_IsAccepted()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));

        var result = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(30)), clock);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("Luz")]
    [InlineData("Este título passa bem do limite de oitenta caracteres que o pedido aceita no Vitrio hoje")]
    public void Create_WithATitleOutOfRange_Fails(string title)
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));

        var result = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(1), title: title), clock);

        Assert.Equal(OrderErrors.InvalidTitle, result.Error);
    }

    [Theory]
    [InlineData(19)]
    [InlineData(501)]
    public void Create_WithADescriptionOutOfRange_Fails(int length)
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));

        var result = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(1), description: new string('a', length)), clock);

        Assert.Equal(OrderErrors.InvalidDescription, result.Error);
    }

    [Fact]
    public void Create_WithSixPhotos_Fails()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));

        var result = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(1), photos: 6), clock);

        Assert.Equal(OrderErrors.TooManyPhotos, result.Error);
    }

    [Fact]
    public void Create_WithPhotos_KeepsThemProcessing()
    {
        var clock = ClockAt(Local(2026, 11, 6, 10));

        var order = Order.Create(CustomerId, ProviderId, Request(Today(clock).AddDays(1), photos: 2), clock).Value;

        Assert.Equal(2, order.Photos.Count);
        Assert.All(order.Photos, photo => Assert.Equal(OrderPhotoStatus.Processing, photo.Status));
    }
}
