using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public sealed record EmailAddress
{
    public const int MaxLength = 254;

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress> Create(string? input)
    {
        var normalized = input?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalized) || normalized.Length > MaxLength || !HasValidShape(normalized))
        {
            return AccountErrors.InvalidEmail;
        }

        return new EmailAddress(normalized);
    }

    public override string ToString() => Value;

    private static bool HasValidShape(string email)
    {
        var at = email.IndexOf('@', StringComparison.Ordinal);
        if (at <= 0 || at != email.LastIndexOf('@') || at > 64 || email.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var domain = email[(at + 1)..];
        var dot = domain.LastIndexOf('.');
        return dot > 0 && dot < domain.Length - 2 && !domain.StartsWith('.') && !domain.Contains("..", StringComparison.Ordinal);
    }
}
