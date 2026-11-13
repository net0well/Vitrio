using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;
using Vitrio.Domain.Tests.Support;

namespace Vitrio.Domain.Tests.Providers;

public sealed class ProviderStatsTests
{
    private readonly TestClock _clock = new(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void UpdateResponseTime_WhenTheBucketChanges_RaisesStatsUpdated()
    {
        var provider = NewProvider();

        provider.UpdateResponseTime(Minutes(5, 8, 10, 12, 14, 30));

        Assert.Equal("15min", provider.ResponseTimeBucket);
        Assert.Equal(6, provider.ResponseSampleSize);
        var raised = Assert.IsType<ProviderStatsUpdated>(Assert.Single(provider.DomainEvents));
        Assert.Equal(provider.Id, raised.ProviderId);
    }

    [Fact]
    public void UpdateResponseTime_WhenOnlyTheSampleGrows_RaisesNothing()
    {
        var provider = NewProvider();
        provider.UpdateResponseTime(Minutes(40, 45, 50, 55, 60));
        provider.ClearDomainEvents();

        provider.UpdateResponseTime(Minutes(40, 45, 50, 50, 55, 60, 60));

        Assert.Equal(7, provider.ResponseSampleSize);
        Assert.Empty(provider.DomainEvents);
    }

    [Fact]
    public void UpdateCompletedOrders_WhenTheCountChanges_RaisesStatsUpdated()
    {
        var provider = NewProvider();

        provider.UpdateCompletedOrders(3);
        provider.UpdateCompletedOrders(3);

        Assert.Equal(3, provider.CompletedOrders);
        Assert.Single(provider.DomainEvents);
    }

    [Fact]
    public void UpdateLateCancellations_KeepsTheCountWithoutTouchingThePublicProfile()
    {
        var provider = NewProvider();

        provider.UpdateLateCancellations(2);

        Assert.Equal(2, provider.LateCancellations90d);
        Assert.Empty(provider.DomainEvents);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void UpdateCompletedOrders_WithANegativeCount_Throws(int count)
    {
        var provider = NewProvider();

        Assert.Throws<ArgumentOutOfRangeException>(() => provider.UpdateCompletedOrders(count));
    }

    private static TimeSpan[] Minutes(params int[] values) => [.. values.Select(value => TimeSpan.FromMinutes(value))];

    private Provider NewProvider() =>
        Provider.Create(Guid.NewGuid(), DisplayName.Create("Carlos Eletricista").Value, "Eletricista", PhoneNumber.Create("16 99123-4567").Value, _clock).Value;
}
