using Vitrio.Domain.Orders;

namespace Vitrio.Domain.Tests.Orders;

public sealed class OrderLimitsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(9, 2)]
    public void Check_BelowBothLimits_Succeeds(int openOrders, int waitingInCategory) =>
        Assert.True(OrderLimits.Check(openOrders, waitingInCategory).IsSuccess);

    [Fact]
    public void Check_WithTenOpenOrders_FailsWithTheTotalLimit() =>
        Assert.Equal(OrderErrors.TooManyOpenOrders, OrderLimits.Check(10, 0).Error);

    [Fact]
    public void Check_WithThreeWaitingInTheCategory_FailsWithTheCategoryLimit() =>
        Assert.Equal(OrderErrors.TooManyWaitingInCategory, OrderLimits.Check(5, 3).Error);
}
