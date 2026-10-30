using Vitrio.Domain.Accounts;
using Vitrio.Shared.Time;

namespace Vitrio.Domain.Tests.Accounts;

public sealed class SessionTests
{
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void RefreshToken_ExpiresAfterSixtyDaysWithoutUse()
    {
        var session = Session.Start(Guid.CreateVersion7(), "Galaxy A54", _clock);

        var token = RefreshToken.IssueFor(session, "hash", _clock);

        Assert.Equal(_clock.UtcNow.AddDays(60), token.ExpiresAt);
        Assert.Equal(_clock.UtcNow.AddDays(365), token.AbsoluteExpiresAt);
        Assert.Equal(session.Id, token.FamilyId);
    }

    [Fact]
    public void RefreshToken_NeverOutlivesOneYearFromLogin()
    {
        var session = Session.Start(Guid.CreateVersion7(), "Galaxy A54", _clock);
        _clock.Advance(TimeSpan.FromDays(340));

        var token = RefreshToken.IssueFor(session, "hash", _clock);

        Assert.Equal(session.CreatedAt.AddDays(365), token.ExpiresAt);
    }

    [Theory]
    [InlineData(null, "Aparelho")]
    [InlineData("   ", "Aparelho")]
    [InlineData("  Galaxy   A54 ", "Galaxy A54")]
    public void Start_NormalizesTheDeviceName(string? deviceName, string expected)
    {
        Assert.Equal(expected, Session.Start(Guid.CreateVersion7(), deviceName, _clock).DeviceName);
    }

    [Fact]
    public void Revoke_KeepsTheFirstReason()
    {
        var session = Session.Start(Guid.CreateVersion7(), "Galaxy A54", _clock);

        session.Revoke("logout", _clock);
        session.Revoke("password_reset", _clock);

        Assert.Equal("logout", session.RevokedReason);
    }

    [Fact]
    public void IsActive_EndsAfterSixtyDaysWithoutUse()
    {
        var session = Session.Start(Guid.CreateVersion7(), "Galaxy A54", _clock);
        _clock.Advance(TimeSpan.FromDays(59));
        session.Touch(_clock);
        _clock.Advance(TimeSpan.FromDays(59));

        Assert.True(session.IsActive(_clock));
        _clock.Advance(TimeSpan.FromDays(2));
        Assert.False(session.IsActive(_clock));
    }

    [Fact]
    public void IsActive_EndsOneYearAfterLoginEvenWhenUsed()
    {
        var session = Session.Start(Guid.CreateVersion7(), "Galaxy A54", _clock);
        _clock.Advance(TimeSpan.FromDays(365));
        session.Touch(_clock);

        Assert.False(session.IsActive(_clock));
    }

    [Fact]
    public void IsActive_IsFalseOnceRevoked()
    {
        var session = Session.Start(Guid.CreateVersion7(), "Galaxy A54", _clock);

        session.Revoke(SessionRevokedReasons.SignedOut, _clock);

        Assert.False(session.IsActive(_clock));
    }

    [Fact]
    public void RefreshToken_OnceUsedOrRevoked_IsNoLongerUsable()
    {
        var session = Session.Start(Guid.CreateVersion7(), "Galaxy A54", _clock);
        var used = RefreshToken.IssueFor(session, "hash-1", _clock);
        var revoked = RefreshToken.IssueFor(session, "hash-2", _clock);

        used.MarkUsed(revoked.Id, _clock);
        revoked.Revoke(_clock);

        Assert.False(used.IsUsable(_clock));
        Assert.Equal(revoked.Id, used.ReplacedById);
        Assert.False(revoked.IsUsable(_clock));
    }

    [Fact]
    public void RefreshToken_AfterSixtyDays_IsNoLongerUsable()
    {
        var session = Session.Start(Guid.CreateVersion7(), "Galaxy A54", _clock);
        var token = RefreshToken.IssueFor(session, "hash", _clock);
        _clock.Advance(TimeSpan.FromDays(59));
        Assert.True(token.IsUsable(_clock));

        _clock.Advance(TimeSpan.FromDays(2));

        Assert.False(token.IsUsable(_clock));
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; private set; } = now;

        public void Advance(TimeSpan by) => UtcNow += by;
    }
}
