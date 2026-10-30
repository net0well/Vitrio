namespace Vitrio.Domain.Providers;

public static class RatingRule
{
    public const int MinReviews = 3;

    public static bool IsRated(decimal? score, int reviewCount) => reviewCount >= MinReviews && score is not null;
}
