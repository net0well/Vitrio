namespace Vitrio.Application.Providers.SetAvailability;

public sealed record AvailabilityInput(DayOfWeek DayOfWeek, string? Start, string? End);
