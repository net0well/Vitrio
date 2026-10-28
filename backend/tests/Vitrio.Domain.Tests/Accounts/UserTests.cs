using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Domain.Tests.Accounts;

public sealed class UserTests
{
    private static readonly EmailAddress Email = EmailAddress.Create("mariana@example.com").Value;
    private static readonly DisplayName Name = DisplayName.Create("Mariana Ferreira").Value;

    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Register_CreatesAPendingAccount()
    {
        var user = User.Register(Email, Name, "password-hash", "2026-10", _clock);

        Assert.Equal(UserStatus.PendingVerification, user.Status);
        Assert.Equal("mariana@example.com", user.Email);
        Assert.Null(user.EmailVerifiedAt);
        Assert.Equal("2026-10", user.AcceptedTermsVersion);
        Assert.Equal(_clock.UtcNow, user.TermsAcceptedAt);
    }

    [Fact]
    public void ConfirmEmail_ActivatesAPendingAccount()
    {
        var user = User.Register(Email, Name, "password-hash", "2026-10", _clock);

        var result = user.ConfirmEmail(_clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(_clock.UtcNow, user.EmailVerifiedAt);
    }

    [Fact]
    public void ConfirmEmail_OnActiveAccount_Fails()
    {
        var user = User.Register(Email, Name, "password-hash", "2026-10", _clock);
        user.ConfirmEmail(_clock);

        Assert.Equal(AccountErrors.NotPending, user.ConfirmEmail(_clock).Error);
    }

    [Fact]
    public void UpdatePendingRegistration_ReplacesNamePasswordAndTerms()
    {
        var user = User.Register(Email, Name, "password-hash", "2026-10", _clock);

        var result = user.UpdatePendingRegistration(DisplayName.Create("Mariana Souza").Value, "new-hash", "2026-11", _clock);

        Assert.True(result.IsSuccess);
        Assert.Equal("Mariana Souza", user.DisplayName);
        Assert.Equal("new-hash", user.PasswordHash);
        Assert.Equal("2026-11", user.AcceptedTermsVersion);
    }

    [Fact]
    public void UpdatePendingRegistration_OnActiveAccount_Fails()
    {
        var user = User.Register(Email, Name, "password-hash", "2026-10", _clock);
        user.ConfirmEmail(_clock);

        var result = user.UpdatePendingRegistration(Name, "new-hash", "2026-10", _clock);

        Assert.Equal(AccountErrors.NotPending, result.Error);
        Assert.Equal("password-hash", user.PasswordHash);
    }

    [Fact]
    public void ShouldNotifyRegistrationAttempt_OncePerDay()
    {
        var user = User.Register(Email, Name, "password-hash", "2026-10", _clock);
        user.ConfirmEmail(_clock);

        Assert.True(user.TryMarkRegistrationAttemptNotified(_clock));
        Assert.False(user.TryMarkRegistrationAttemptNotified(_clock));

        _clock.Advance(TimeSpan.FromHours(24));
        Assert.True(user.TryMarkRegistrationAttemptNotified(_clock));
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; private set; } = now;

        public void Advance(TimeSpan by) => UtcNow += by;
    }
}
