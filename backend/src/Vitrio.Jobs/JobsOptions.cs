namespace Vitrio.Jobs;

public sealed class JobsOptions
{
    public const string SectionName = "Jobs";

    public bool Enabled { get; set; } = true;

    public string Schema { get; set; } = "hangfire";
}
