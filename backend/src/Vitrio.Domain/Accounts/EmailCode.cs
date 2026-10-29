using System.Security.Cryptography;
using System.Text;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Domain.Accounts;

public sealed class EmailCode
{
    public const int MaxAttempts = 5;
    public const int MaxIssuedPerWindow = 5;

    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan ResendInterval = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan IssueWindow = TimeSpan.FromHours(1);

    private EmailCode()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public EmailCodePurpose Purpose { get; private set; }

    public string CodeHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public static EmailCode Issue(EmailAddress email, EmailCodePurpose purpose, string codeHash, IClock clock)
    {
        var now = clock.UtcNow;
        return new EmailCode
        {
            Id = Guid.CreateVersion7(now),
            Email = email.Value,
            Purpose = purpose,
            CodeHash = codeHash,
            CreatedAt = now,
            ExpiresAt = now + Lifetime
        };
    }

    public bool IsUsable(IClock clock) => ConsumedAt is null && Attempts < MaxAttempts && clock.UtcNow < ExpiresAt;

    public Result Verify(string candidateHash, IClock clock)
    {
        if (!IsUsable(clock))
        {
            return AccountErrors.InvalidCode;
        }

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(CodeHash), Encoding.UTF8.GetBytes(candidateHash)))
        {
            Attempts++;
            return AccountErrors.InvalidCode;
        }

        ConsumedAt = clock.UtcNow;
        return Result.Success();
    }

    public void CountFailedAttempt()
    {
        if (Attempts < MaxAttempts)
        {
            Attempts++;
        }
    }

    public void Invalidate(IClock clock) => ConsumedAt ??= clock.UtcNow;
}
