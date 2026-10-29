using System.Collections.Frozen;
using System.Globalization;
using System.Text;

namespace Vitrio.Application.Accounts.Common;

public static class PasswordPolicy
{
    public const int MinLength = 10;
    public const int MaxLength = 128;

    private const string CommonPasswordsResource = "Vitrio.Application.CommonPasswords.txt";

    private static readonly Lazy<FrozenSet<string>> CommonPasswords = new(LoadCommonPasswords);

    public static string Normalize(string password) => password.Normalize(NormalizationForm.FormKC);

    public static PasswordProblem? FindProblem(string? password, string? email)
    {
        var normalized = Normalize(password ?? string.Empty);
        var length = new StringInfo(normalized).LengthInTextElements;
        if (length < MinLength)
        {
            return PasswordProblem.TooShort;
        }

        if (length > MaxLength)
        {
            return PasswordProblem.TooLong;
        }

        if (CommonPasswords.Value.Contains(normalized.ToLowerInvariant()))
        {
            return PasswordProblem.Common;
        }

        if (string.Equals(normalized.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return PasswordProblem.SameAsEmail;
        }

        return null;
    }

    private static FrozenSet<string> LoadCommonPasswords()
    {
        using var stream = typeof(PasswordPolicy).Assembly.GetManifestResourceStream(CommonPasswordsResource)
            ?? throw new InvalidOperationException($"Embedded resource '{CommonPasswordsResource}' was not found.");
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var passwords = new HashSet<string>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            if (line.Length > 0)
            {
                passwords.Add(line);
            }
        }

        return passwords.ToFrozenSet(StringComparer.Ordinal);
    }
}
