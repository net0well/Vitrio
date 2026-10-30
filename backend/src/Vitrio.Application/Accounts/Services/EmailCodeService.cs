using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Accounts.Services;

internal sealed class EmailCodeService(
    IEmailCodeWritableRepository emailCodes,
    IUserSearchableRepository users,
    IVerificationCodes codes,
    IClock clock) : IEmailCodeService
{
    public async Task<Result<string>> IssueAsync(EmailAddress email, EmailCodePurpose purpose, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var recent = await emailCodes.ListIssuedSinceAsync(email, now - EmailCode.IssueWindow, cancellationToken);

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

    public async Task<Result<AccountEmail?>> PrepareAsync(
        EmailAddress email,
        EmailCodePurpose purpose,
        CancellationToken cancellationToken)
    {
        var code = await IssueAsync(email, purpose, cancellationToken);
        if (code.IsFailure)
        {
            return code.Error;
        }

        var recipientStatus = purpose == EmailCodePurpose.VerifyEmail ? UserStatus.PendingVerification : UserStatus.Active;
        var displayName = await users.FindDisplayNameAsync(email, recipientStatus, cancellationToken);
        if (displayName is null)
        {
            return Result<AccountEmail?>.Success(null);
        }

        var firstName = DisplayName.FromStored(displayName).FirstName;
        return purpose == EmailCodePurpose.VerifyEmail
            ? new VerificationCodeEmail(email.Value, firstName, code.Value)
            : new PasswordResetCodeEmail(email.Value, firstName, code.Value);
    }

    public bool Matches(EmailCode emailCode, EmailAddress email, string candidate) =>
        emailCode.Verify(codes.Hash(email, emailCode.Purpose, candidate), clock).IsSuccess;

    private static TimeSpan WaitBeforeNextCode(IReadOnlyList<EmailCode> recent, DateTimeOffset now)
    {
        if (recent.Count == 0)
        {
            return TimeSpan.Zero;
        }

        var wait = recent[^1].CreatedAt + EmailCode.ResendInterval - now;
        if (recent.Count >= EmailCode.MaxIssuedPerWindow)
        {
            var oldestCounted = recent[^EmailCode.MaxIssuedPerWindow].CreatedAt + EmailCode.IssueWindow - now;
            wait = oldestCounted > wait ? oldestCounted : wait;
        }

        return wait;
    }
}
