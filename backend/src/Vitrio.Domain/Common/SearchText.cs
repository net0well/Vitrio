using System.Globalization;
using System.Text;

namespace Vitrio.Domain.Common;

public static class SearchText
{
    public static string Normalize(string? text)
    {
        var decomposed = FreeText.Normalize(text).Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
