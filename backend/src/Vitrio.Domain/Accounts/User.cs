using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public sealed class User : AggregateRoot
{
    public const int TermsVersionMaxLength = 20;
    public const int MaxFailedSignIns = 5;

    public static readonly TimeSpan FailedSignInWindow = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private static readonly TimeSpan RegistrationAttemptNoticeInterval = TimeSpan.FromHours(24);

    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public DateTimeOffset PasswordChangedAt { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTimeOffset? EmailVerifiedAt { get; private set; }

    public string AcceptedTermsVersion { get; private set; } = string.Empty;

    public DateTimeOffset TermsAcceptedAt { get; private set; }

    public DateTimeOffset? RegistrationAttemptNotifiedAt { get; private set; }

    public int FailedSignInCount { get; private set; }

    public DateTimeOffset? FailedSignInWindowStartedAt { get; private set; }

    public DateTimeOffset? LockedOutUntil { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static User Register(EmailAddress email, DisplayName displayName, string passwordHash, string termsVersion, IClock clock)
    {
        var now = clock.UtcNow;
        return new User
        {
            Id = Guid.CreateVersion7(now),
            Email = email.Value,
            DisplayName = displayName.Value,
            PasswordHash = passwordHash,
            PasswordChangedAt = now,
            Status = UserStatus.PendingVerification,
            AcceptedTermsVersion = termsVersion,
            TermsAcceptedAt = now,
            CreatedAt = now
        };
    }

    public Result UpdatePendingRegistration(DisplayName displayName, string passwordHash, string termsVersion, IClock clock)
    {
        if (Status != UserStatus.PendingVerification)
        {
            return AccountErrors.NotPending;
        }

        var now = clock.UtcNow;
        DisplayName = displayName.Value;
        PasswordHash = passwordHash;
        PasswordChangedAt = now;
        AcceptedTermsVersion = termsVersion;
        TermsAcceptedAt = now;
        return Result.Success();
    }

    public Result ConfirmEmail(IClock clock)
    {
        if (Status != UserStatus.PendingVerification)
        {
            return AccountErrors.NotPending;
        }

        Status = UserStatus.Active;
        EmailVerifiedAt = clock.UtcNow;
        return Result.Success();
    }

    public TimeSpan LockoutRemaining(IClock clock) =>
        LockedOutUntil is { } until && until > clock.UtcNow ? until - clock.UtcNow : TimeSpan.Zero;

    public bool RegisterFailedSignIn(IClock clock)
    {
        var now = clock.UtcNow;
        if (FailedSignInWindowStartedAt is not { } startedAt || now - startedAt >= FailedSignInWindow)
        {
            FailedSignInWindowStartedAt = now;
            FailedSignInCount = 0;
        }

        FailedSignInCount++;
        if (FailedSignInCount < MaxFailedSignIns)
        {
            return false;
        }

        LockedOutUntil = now + LockoutDuration;
        FailedSignInCount = 0;
        FailedSignInWindowStartedAt = null;
        return true;
    }

    public void RegisterSuccessfulSignIn()
    {
        FailedSignInCount = 0;
        FailedSignInWindowStartedAt = null;
        LockedOutUntil = null;
    }

    public void ReplacePasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void ChangePassword(string passwordHash, IClock clock)
    {
        PasswordHash = passwordHash;
        PasswordChangedAt = clock.UtcNow;
        RegisterSuccessfulSignIn();
    }

    public bool TryMarkRegistrationAttemptNotified(IClock clock)
    {
        var now = clock.UtcNow;
        if (RegistrationAttemptNotifiedAt is { } last && now - last < RegistrationAttemptNoticeInterval)
        {
            return false;
        }

        RegistrationAttemptNotifiedAt = now;
        return true;
    }
}
