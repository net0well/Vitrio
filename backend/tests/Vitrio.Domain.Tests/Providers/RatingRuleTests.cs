using Vitrio.Domain.Providers;

namespace Vitrio.Domain.Tests.Providers;

public sealed class RatingRuleTests
{
    [Fact]
    public void IsRated_WithThreeReviewsAndAScore_IsTrue() => Assert.True(RatingRule.IsRated(4.5m, 3));

    [Fact]
    public void IsRated_WithFewerThanThreeReviews_IsFalse() => Assert.False(RatingRule.IsRated(5m, 2));

    [Fact]
    public void IsRated_WithoutAScore_IsFalse() => Assert.False(RatingRule.IsRated(null, 10));
}
