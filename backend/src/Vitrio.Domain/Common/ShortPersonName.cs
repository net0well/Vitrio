namespace Vitrio.Domain.Common;

public static class ShortPersonName
{
    public static string From(string fullName)
    {
        var parts = FreeText.Normalize(fullName).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => string.Empty,
            1 => parts[0],
            _ => $"{parts[0]} {char.ToUpperInvariant(parts[^1][0])}."
        };
    }
}
