namespace Vitrio.Domain.Providers;

public sealed record AvailabilityDraft(DayOfWeek DayOfWeek, int StartMinute, int EndMinute);
