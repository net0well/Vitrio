using System.Globalization;
using Vitrio.Domain.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Domain.Accounts;

public sealed record DisplayName
{
    public const int MinLength = 2;
    public const int MaxLength = 60;

    private static readonly CultureInfo PortugueseCulture = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly HashSet<string> SurnameParticles =
        new(StringComparer.OrdinalIgnoreCase) { "da", "das", "de", "di", "do", "dos", "du", "e" };

    private DisplayName(string value) => Value = value;

    public string Value { get; }

    public string FirstName => Value.Split(' ')[0];

    public string FirstNameWithInitial =>
        Surname is { } surname ? $"{FirstName} {char.ToUpper(surname[0], PortugueseCulture)}." : FirstName;

    public string Initials =>
        $"{FirstName[0]}{(Surname is { } surname ? surname[0] : string.Empty)}".ToUpper(PortugueseCulture);

    private string? Surname => Value.Split(' ').Skip(1).FirstOrDefault(part => !SurnameParticles.Contains(part));

    public static Result<DisplayName> Create(string? input)
    {
        var normalized = FreeText.Normalize(input);
        if (normalized.Length is < MinLength or > MaxLength)
        {
            return AccountErrors.InvalidDisplayName;
        }

        if (FreeText.ContainsContactInformation(normalized))
        {
            return AccountErrors.DisplayNameWithContact;
        }

        return new DisplayName(normalized);
    }

    public static DisplayName FromStored(string value) => new(value);

    public override string ToString() => Value;
}
