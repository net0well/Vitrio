using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Common;

public sealed class EmailCodeIssuer(IAppDbContext db, IVerificationCodes codes, IClock clock)
{
    public const int MaxCodesPerWindow = 5;

    public static readonly TimeSpan ResendInterval = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan Window = TimeSpan.FromHours(1);

    public async Task<Result<string>> IssueAsync(EmailAddress email, EmailCodePurpose purpose, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var windowStart = now - Window;
        var recent = await db.EmailCodes
            .Where(code => code.Email == email.Value && code.CreatedAt > windowStart)
            .OrderBy(code => code.CreatedAt)
            .ToListAsync(cancellationToken);

        var wait = WaitBeforeNextCode(recent, now);
        if (wait > TimeSpan.Zero)
        {
            return AccountErrors.TooManyCodes(wait);
        }

        foreach (var previous in recent.Where(code => code.Purpose == purpose))
        {
            previous.Invalidate(clock);
        }

        var value = codes.Generate();
        db.EmailCodes.Add(EmailCode.Issue(email, purpose, codes.Hash(email, purpose, value), clock));
        return value;
    }

    private static TimeSpan WaitBeforeNextCode(List<EmailCode> recent, DateTimeOffset now)
    {
        if (recent.Count == 0)
        {
            return TimeSpan.Zero;
        }

        var wait = recent[^1].CreatedAt + ResendInterval - now;
        if (recent.Count >= MaxCodesPerWindow)
        {
            var oldestCounted = recent[^MaxCodesPerWindow].CreatedAt + Window - now;
            wait = oldestCounted > wait ? oldestCounted : wait;
        }

        return wait;
    }
}
