using Vitrio.Shared.Results;

namespace Vitrio.Domain.Providers;

public sealed record Cnpj
{
    public const int Length = 14;

    private static readonly int[] FirstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] SecondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private Cnpj(string value) => Value = value;

    public string Value { get; }

    public static Result<Cnpj> Create(string? input)
    {
        var digits = new string((input ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
        if (digits.Length != Length || digits.Distinct().Count() == 1
            || CheckDigit(digits[..12], FirstWeights) != digits[12] - '0'
            || CheckDigit(digits[..13], SecondWeights) != digits[13] - '0')
        {
            return ProviderErrors.InvalidCnpj;
        }

        return new Cnpj(digits);
    }

    public static Cnpj FromStored(string value) => new(value);

    public override string ToString() => Value;

    private static int CheckDigit(string digits, int[] weights)
    {
        var remainder = digits.Select((digit, index) => (digit - '0') * weights[index]).Sum() % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
