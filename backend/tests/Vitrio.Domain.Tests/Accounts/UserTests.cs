using Vitrio.Domain.Accounts;
using Vitrio.Shared.Time;

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

    [Fact]
    public void RegisterFailedSignIn_FifthFailureWithinFifteenMinutes_LocksForFifteenMinutes()
    {
        var user = ActiveUser();
        for (var failure = 1; failure < User.MaxFailedSignIns; failure++)
        {
            Assert.False(user.RegisterFailedSignIn(_clock));
            _clock.Advance(TimeSpan.FromMinutes(2));
        }

        Assert.True(user.RegisterFailedSignIn(_clock));

        Assert.Equal(User.LockoutDuration, user.LockoutRemaining(_clock));
        _clock.Advance(User.LockoutDuration);
        Assert.Equal(TimeSpan.Zero, user.LockoutRemaining(_clock));
    }

    [Fact]
    public void RegisterFailedSignIn_SpreadOverMoreThanFifteenMinutes_DoesNotLock()
    {
        var user = ActiveUser();
        for (var failure = 0; failure < User.MaxFailedSignIns * 2; failure++)
        {
            Assert.False(user.RegisterFailedSignIn(_clock));
            _clock.Advance(TimeSpan.FromMinutes(4));
        }

        Assert.Equal(TimeSpan.Zero, user.LockoutRemaining(_clock));
    }

    [Fact]
    public void RegisterSuccessfulSignIn_ClearsFailuresAndLockout()
    {
        var user = ActiveUser();
        for (var failure = 0; failure < User.MaxFailedSignIns; failure++)
        {
            user.RegisterFailedSignIn(_clock);
        }

        user.RegisterSuccessfulSignIn();

        Assert.Equal(TimeSpan.Zero, user.LockoutRemaining(_clock));
        Assert.Equal(0, user.FailedSignInCount);
        Assert.False(user.RegisterFailedSignIn(_clock));
    }

    [Fact]
    public void ChangePassword_ReplacesTheHashAndUnlocksTheAccount()
    {
        var user = ActiveUser();
        for (var failure = 0; failure < User.MaxFailedSignIns; failure++)
        {
            user.RegisterFailedSignIn(_clock);
        }

        _clock.Advance(TimeSpan.FromMinutes(1));
        user.ChangePassword("new-hash", _clock);

        Assert.Equal("new-hash", user.PasswordHash);
        Assert.Equal(_clock.UtcNow, user.PasswordChangedAt);
        Assert.Equal(TimeSpan.Zero, user.LockoutRemaining(_clock));
    }

    [Fact]
    public void Delete_AnonymizesTheAccountAndRaisesUserDeleted()
    {
        var user = ActiveUser();
        user.ChangePhoneNumber(PhoneNumber.Create("16991234567").Value);

        var result = user.Delete(_clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Deleted, user.Status);
        Assert.Equal($"{user.Id:N}@removido.invalid", user.Email);
        Assert.Equal(User.RemovedDisplayName, user.DisplayName);
        Assert.Null(user.PhoneNumber);
        Assert.Empty(user.PasswordHash);
        Assert.Equal(_clock.UtcNow, user.DeletedAt);
        Assert.Equal(new UserDeleted(user.Id) { EventId = user.DomainEvents.Single().EventId }, user.DomainEvents.Single());
    }

    [Fact]
    public void Delete_Twice_Fails()
    {
        var user = ActiveUser();
        user.Delete(_clock);

        Assert.True(user.Delete(_clock).IsFailure);
    }

    private User ActiveUser()
    {
        var user = User.Register(
            EmailAddress.Create("mariana@example.com").Value,
            DisplayName.Create("Mariana Souza").Value,
            "hash",
            "2026-10-28",
            _clock);
        user.ConfirmEmail(_clock);
        return user;
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; private set; } = now;

        public void Advance(TimeSpan by) => UtcNow += by;
    }
}
