using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Services;

public sealed class EmailCodeService(IEmailCodeWritableRepository emailCodes, IVerificationCodes codes, IClock clock)
{
    public const int MaxCodesPerWindow = 5;

    public static readonly TimeSpan ResendInterval = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan Window = TimeSpan.FromHours(1);

    public async Task<Result<string>> IssueAsync(EmailAddress email, EmailCodePurpose purpose, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var recent = await emailCodes.ListIssuedSinceAsync(email, now - Window, cancellationToken);

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
        emailCodes.Add(EmailCode.Issue(email, purpose, codes.Hash(email, purpose, value), clock));
        return value;
    }

    public bool Matches(EmailCode emailCode, EmailAddress email, string candidate) =>
        emailCode.Verify(codes.Hash(email, emailCode.Purpose, candidate), clock).IsSuccess;

    private static TimeSpan WaitBeforeNextCode(IReadOnlyList<EmailCode> recent, DateTimeOffset now)
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
