namespace Vitrio.Infrastructure.Events;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public bool Enabled { get; set; } = true;

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    public int BatchSize { get; set; } = 50;

    public TimeSpan LockDuration { get; set; } = TimeSpan.FromSeconds(60);

    public TimeSpan StatisticsInterval { get; set; } = TimeSpan.FromSeconds(15);
}
