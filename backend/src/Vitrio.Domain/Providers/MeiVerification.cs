namespace Vitrio.Domain.Providers;

public sealed class MeiVerification
{
    public const int CompanyNameMaxLength = 150;

    private MeiVerification()
    {
    }

    public Guid ProviderId { get; private set; }

    public string Cnpj { get; private set; } = string.Empty;

    public MeiStatus Status { get; private set; }

    public DateTimeOffset CheckedAt { get; private set; }

    public DateTimeOffset? PendingSince { get; private set; }

    public string? CompanyName { get; private set; }

    public string? ReasonCode { get; private set; }

    internal static MeiVerification Start(Guid providerId, Cnpj cnpj, DateTimeOffset now) => new()
    {
        ProviderId = providerId,
        Cnpj = cnpj.Value,
        Status = MeiStatus.Pending,
        CheckedAt = now
    };

    internal void Restart(Cnpj cnpj, DateTimeOffset now)
    {
        Cnpj = cnpj.Value;
        Status = MeiStatus.Pending;
        PendingSince = null;
        CompanyName = null;
        ReasonCode = null;
        CheckedAt = now;
    }

    internal void Verify(string? companyName, DateTimeOffset now)
    {
        Status = MeiStatus.Verified;
        CompanyName = Truncate(companyName) ?? CompanyName;
        PendingSince = null;
        ReasonCode = null;
        CheckedAt = now;
    }

    internal void MarkPending(DateTimeOffset now)
    {
        PendingSince ??= now;
        Status = MeiStatus.Pending;
        ReasonCode = MeiReasons.LookupUnavailable;
        CheckedAt = now;
    }

    internal void Fail(string reason, string? companyName, DateTimeOffset now)
    {
        Status = MeiStatus.Failed;
        CompanyName = Truncate(companyName) ?? CompanyName;
        PendingSince = null;
        ReasonCode = reason;
        CheckedAt = now;
    }

    internal void Lose(string reason, DateTimeOffset now)
    {
        Status = MeiStatus.Lost;
        ReasonCode = reason;
        CheckedAt = now;
    }

    private static string? Truncate(string? companyName) =>
        string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim()[..Math.Min(companyName.Trim().Length, CompanyNameMaxLength)];
}
