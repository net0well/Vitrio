using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Vitrio.Domain.Common;

public static partial class FreeText
{
    public static string Normalize(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(input.Length);
        foreach (var character in input.Normalize(NormalizationForm.FormC))
        {
            var category = char.GetUnicodeCategory(character);
            if (char.IsWhiteSpace(character))
            {
                builder.Append(' ');
            }
            else if (category is not (UnicodeCategory.Control or UnicodeCategory.Format))
            {
                builder.Append(character);
            }
        }

        return Whitespace().Replace(builder.ToString(), " ").Trim();
    }

    public static bool ContainsContactInformation(string text) =>
        Email().IsMatch(text) || Link().IsMatch(text) || Phone().IsMatch(text);

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"[^\s@]+@[^\s@]+\.[^\s@]+")]
    private static partial Regex Email();

    [GeneratedRegex(@"(https?://|www\.)|\b[\p{L}\p{N}-]+\.(com|net|org|br|app|dev|io|me|info)\b", RegexOptions.IgnoreCase)]
    private static partial Regex Link();

    [GeneratedRegex(@"(\d[\s().\-]*){8,}")]
    private static partial Regex Phone();
}
