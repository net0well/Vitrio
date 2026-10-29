using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;

namespace Vitrio.Domain.Tests.Providers;

public sealed class AvailableTodayRuleTests
{
    private const string SaoPaulo = "America/Sao_Paulo";

    [Fact]
    public void IsOn_SameLocalDay_IsTrue()
    {
        var setAt = new DateTimeOffset(2026, 10, 29, 11, 0, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 10, 30, 2, 59, 0, TimeSpan.Zero);

        Assert.True(AvailableTodayRule.IsOn(true, setAt, now, SaoPaulo));
    }

    [Fact]
    public void IsOn_AfterLocalMidnight_IsFalseWithoutAnyJob()
    {
        var setAt = new DateTimeOffset(2026, 10, 29, 11, 0, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 10, 30, 3, 0, 0, TimeSpan.Zero);

        Assert.False(AvailableTodayRule.IsOn(true, setAt, now, SaoPaulo));
    }

    [Fact]
    public void IsOn_WhenTurnedOff_IsFalse()
    {
        var setAt = new DateTimeOffset(2026, 10, 29, 11, 0, 0, TimeSpan.Zero);

        Assert.False(AvailableTodayRule.IsOn(false, setAt, setAt, SaoPaulo));
    }

    [Fact]
    public void ToPublicCell_MovesThePointToTheCenterOfAHalfKilometerCell()
    {
        var home = new Coordinates(-21.25471, -48.32213);
        var neighbor = new Coordinates(-21.25390, -48.32150);

        var cell = home.ToPublicCell();

        Assert.Equal(cell, neighbor.ToPublicCell());
        Assert.InRange(Math.Abs(cell.Latitude - home.Latitude), 0, 0.0045);
        Assert.InRange(Math.Abs(cell.Longitude - home.Longitude), 0, 0.0045 / Math.Cos(cell.Latitude * Math.PI / 180));
    }
}
