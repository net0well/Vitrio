using Vitrio.Domain.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Domain.Orders;

public sealed record ServiceSchedule
{
    public const int StepMinutes = 30;
    public const int MinimumDurationMinutes = 30;
    public const int MaximumDurationMinutes = 480;

    private const int MinutesPerDay = 24 * 60;

    private ServiceSchedule()
    {
    }

    public DateOnly Date { get; private init; }

    public TimeOnly Start { get; private init; }

    public int DurationMinutes { get; private init; }

    public DateTimeOffset StartsAt { get; private init; }

    public DateTimeOffset EndsAt { get; private init; }

    public TimeOnly End => Start.AddMinutes(DurationMinutes);

    internal ServiceSchedule Copy() => this with { };

    public static Result<ServiceSchedule> Create(DateOnly date, TimeOnly start, int durationMinutes, string timeZoneId)
    {
        if (start.Minute % StepMinutes != 0 || start.Second != 0 || start.Millisecond != 0
            || durationMinutes is < MinimumDurationMinutes or > MaximumDurationMinutes
            || durationMinutes % StepMinutes != 0)
        {
            return OrderErrors.InvalidSchedule;
        }

        if ((start.Hour * 60) + start.Minute + durationMinutes > MinutesPerDay)
        {
            return OrderErrors.ScheduleCrossesMidnight;
        }

        var startsAt = CityTime.ToUtc(date, start, CityTime.Zone(timeZoneId));
        return new ServiceSchedule
        {
            Date = date,
            Start = start,
            DurationMinutes = durationMinutes,
            StartsAt = startsAt,
            EndsAt = startsAt.AddMinutes(durationMinutes)
        };
    }
}
