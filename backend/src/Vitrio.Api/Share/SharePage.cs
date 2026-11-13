using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using Vitrio.Application.Providers.Common;

namespace Vitrio.Api.Share;

public static partial class SharePage
{
    private static readonly HtmlEncoder Html = HtmlEncoder.Create(UnicodeRanges.All);
    private static readonly string Wordmark = Load("Vitrio.Share.vitrio-wordmark.svg").Trim();
    private static readonly string Icon = $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(Load("Vitrio.Share.vitrio-mark.svg").Trim()))}";
    private static readonly string Found = WithBrand(Load("Vitrio.Share.Profile.html"));
    private static readonly string Missing = WithBrand(Load("Vitrio.Share.ProfileMissing.html"));

    public static string Render(SharePageResponse page)
    {
        if (!page.Found)
        {
            return Fill(Missing, new(StringComparer.Ordinal) { ["playStore"] = Encode(page.PlayStoreUrl) });
        }

        var title = $"{page.DisplayName} · {page.Headline}";
        var image = page.ImageUrl is null
            ? string.Empty
            : $"<meta property=\"og:image\" content=\"{Encode(page.ImageUrl)}\">\n  <img class=\"photo\" src=\"{Encode(page.ImageUrl)}\" alt=\"\">";
        return Fill(Found, new(StringComparer.Ordinal)
        {
            ["title"] = Encode(title),
            ["titleAttribute"] = Encode(title),
            ["description"] = Encode($"{page.Headline} em {page.Place}. {page.RatingText}. Peça orçamento pelo Vitrio."),
            ["url"] = Encode(page.Url!),
            ["image"] = image,
            ["name"] = Encode(page.DisplayName!),
            ["headline"] = Encode(page.Headline!),
            ["place"] = Encode(page.Place!),
            ["rating"] = Encode(page.RatingText!),
            ["playStore"] = Encode(page.PlayStoreUrl)
        });
    }

    public static string ContentSecurityPolicy(SharePageResponse page)
    {
        var images = page.ImageUrl is not null && Uri.TryCreate(page.ImageUrl, UriKind.Absolute, out var image)
            ? $"{image.GetLeftPart(UriPartial.Authority)} data:"
            : "data:";
        return $"default-src 'none'; style-src 'unsafe-inline'; img-src {images}; base-uri 'none'; form-action 'none'; frame-ancestors 'none'";
    }

    private static string Encode(string value) => Html.Encode(value);

    // One pass over the template: a value that contains another placeholder is never filled again.
    private static string Fill(string template, Dictionary<string, string> values) =>
        Placeholder().Replace(template, match => values.TryGetValue(match.Groups[1].Value, out var value)
            ? value
            : throw new InvalidOperationException($"The share page has no value for '{match.Groups[1].Value}'."));

    private static string WithBrand(string template) => template
        .Replace("{{wordmark}}", Wordmark, StringComparison.Ordinal)
        .Replace("{{icon}}", Icon, StringComparison.Ordinal);

    private static string Load(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"The page template '{name}' is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex Placeholder();
}
