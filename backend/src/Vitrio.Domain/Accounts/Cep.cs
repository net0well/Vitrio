using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public sealed record Cep
{
    private Cep(string value) => Value = value;

    public string Value { get; }

    public static Result<Cep> Create(string? input)
    {
        var trimmed = (input ?? string.Empty).Trim();
        var digits = trimmed.Replace("-", string.Empty, StringComparison.Ordinal).Replace(".", string.Empty, StringComparison.Ordinal);
        if (digits.Length != 8 || !digits.All(char.IsAsciiDigit) || digits == "00000000")
        {
            return AccountErrors.InvalidCep;
        }

        return new Cep(digits);
    }

    public override string ToString() => Value;
}
