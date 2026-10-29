using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Domain.Tests.Accounts;

public sealed class EmailCodeTests
{
    private static readonly EmailAddress Email = EmailAddress.Create("mariana@example.com").Value;

    private readonly TestClock _clock = new(new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Verify_WithMatchingHashWithinLifetime_ConsumesTheCode()
    {
        var code = EmailCode.Issue(Email, EmailCodePurpose.VerifyEmail, "hash-1", _clock);

        var result = code.Verify("hash-1", _clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(_clock.UtcNow, code.ConsumedAt);
    }

    [Fact]
    public void Verify_AfterTenMinutes_Fails()
    {
        var code = EmailCode.Issue(Email, EmailCodePurpose.VerifyEmail, "hash-1", _clock);
        _clock.Advance(TimeSpan.FromMinutes(10));

        Assert.Equal(AccountErrors.InvalidCode, code.Verify("hash-1", _clock).Error);
    }

    [Fact]
    public void Verify_TwiceWithTheSameCode_FailsTheSecondTime()
    {
        var code = EmailCode.Issue(Email, EmailCodePurpose.VerifyEmail, "hash-1", _clock);
        code.Verify("hash-1", _clock);

        Assert.Equal(AccountErrors.InvalidCode, code.Verify("hash-1", _clock).Error);
    }

    [Fact]
    public void Verify_AfterFiveWrongAttempts_RejectsEvenTheRightCode()
    {
        var code = EmailCode.Issue(Email, EmailCodePurpose.VerifyEmail, "hash-1", _clock);
        for (var attempt = 0; attempt < EmailCode.MaxAttempts; attempt++)
        {
            Assert.True(code.Verify("wrong", _clock).IsFailure);
        }

        Assert.Equal(AccountErrors.InvalidCode, code.Verify("hash-1", _clock).Error);
        Assert.Equal(EmailCode.MaxAttempts, code.Attempts);
    }

    [Fact]
    public void CountFailedAttempt_BeyondTheLimit_StopsAtFiveAndBlocksTheCode()
    {
        var code = EmailCode.Issue(Email, EmailCodePurpose.VerifyEmail, "hash-1", _clock);
        for (var attempt = 0; attempt < EmailCode.MaxAttempts + 1; attempt++)
        {
            code.CountFailedAttempt();
        }

        Assert.Equal(EmailCode.MaxAttempts, code.Attempts);
        Assert.Equal(AccountErrors.InvalidCode, code.Verify("hash-1", _clock).Error);
    }

    [Fact]
    public void Invalidate_MakesTheCodeUnusable()
    {
        var code = EmailCode.Issue(Email, EmailCodePurpose.VerifyEmail, "hash-1", _clock);

        code.Invalidate(_clock);

        Assert.False(code.IsUsable(_clock));
    }

    private sealed class TestClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; private set; } = now;

        public void Advance(TimeSpan by) => UtcNow += by;
    }
}
