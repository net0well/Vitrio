using Vitrio.Domain.Orders;
using Vitrio.Domain.Providers;

namespace Vitrio.Domain.Tests.Providers;

public sealed class ResponseTimeBucketTests
{
    [Theory]
    [InlineData(new[] { 5, 10, 15, 20, 90 }, "15min")]
    [InlineData(new[] { 10, 16, 20, 25, 300 }, "30min")]
    [InlineData(new[] { 20, 31, 40, 60, 60 }, "1h")]
    [InlineData(new[] { 30, 61, 120, 180, 200 }, "3h")]
    [InlineData(new[] { 100, 200, 300, 1440, 2880 }, "1d")]
    public void ResponseTimeBucket_UsesTheMedianOfTheSample(int[] minutes, string expected)
    {
        Assert.Equal(expected, ProviderStats.ResponseTimeBucket(Minutes(minutes)));
    }

    [Fact]
    public void ResponseTimeBucket_WithAnEvenSample_AveragesTheTwoMiddleValues()
    {
        Assert.Equal("30min", ProviderStats.ResponseTimeBucket(Minutes(5, 10, 14, 18, 40, 50)));
    }

    [Fact]
    public void ResponseTimeBucket_WithFewerThanFiveOrders_IsEmpty()
    {
        Assert.Null(ProviderStats.ResponseTimeBucket(Minutes(5, 5, 5, 5)));
    }

    [Fact]
    public void ResponseTimeBucket_WhenTheMedianPassesOneDay_IsEmpty()
    {
        Assert.Null(ProviderStats.ResponseTimeBucket(Minutes(1500, 1600, 1700, 1800, 2880)));
    }

    [Fact]
    public void ResponseDelay_OfAnExpiredOrder_IsTheWholeTermItHad()
    {
        var createdAt = new DateTimeOffset(2026, 10, 29, 16, 0, 0, TimeSpan.Zero);

        Assert.Equal(TimeSpan.FromHours(2), Order.ResponseDelay(createdAt, null, createdAt.AddHours(2)));
        Assert.Equal(TimeSpan.FromMinutes(20), Order.ResponseDelay(createdAt, createdAt.AddMinutes(20), createdAt.AddHours(2)));
    }

    private static TimeSpan[] Minutes(params int[] values) => [.. values.Select(value => TimeSpan.FromMinutes(value))];
}
