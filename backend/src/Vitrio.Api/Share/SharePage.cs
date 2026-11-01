using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Vitrio.Application.Providers.Common;

namespace Vitrio.Api.Share;

public static class SharePage
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
            return Missing.Replace("{{playStore}}", Encode(page.PlayStoreUrl), StringComparison.Ordinal);
        }

        var title = $"{page.DisplayName} · {page.Headline}";
        var image = page.ImageUrl is null
            ? string.Empty
            : $"<meta property=\"og:image\" content=\"{Encode(page.ImageUrl)}\">\n  <img class=\"photo\" src=\"{Encode(page.ImageUrl)}\" alt=\"\">";
        return Found
            .Replace("{{title}}", Encode(title), StringComparison.Ordinal)
            .Replace("{{titleAttribute}}", Encode(title), StringComparison.Ordinal)
            .Replace("{{description}}", Encode($"{page.Headline} em {page.Place}. {page.RatingText}. Peça orçamento pelo Vitrio."), StringComparison.Ordinal)
            .Replace("{{url}}", Encode(page.Url!), StringComparison.Ordinal)
            .Replace("{{image}}", image, StringComparison.Ordinal)
            .Replace("{{name}}", Encode(page.DisplayName!), StringComparison.Ordinal)
            .Replace("{{headline}}", Encode(page.Headline!), StringComparison.Ordinal)
            .Replace("{{place}}", Encode(page.Place!), StringComparison.Ordinal)
            .Replace("{{rating}}", Encode(page.RatingText!), StringComparison.Ordinal)
            .Replace("{{playStore}}", Encode(page.PlayStoreUrl), StringComparison.Ordinal);
    }

    public static string ContentSecurityPolicy(SharePageResponse page)
    {
        var images = page.ImageUrl is not null && Uri.TryCreate(page.ImageUrl, UriKind.Absolute, out var image)
            ? $"{image.GetLeftPart(UriPartial.Authority)} data:"
            : "data:";
        return $"default-src 'none'; style-src 'unsafe-inline'; img-src {images}; base-uri 'none'; form-action 'none'; frame-ancestors 'none'";
    }

    private static string Encode(string value) => Html.Encode(value);

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
}
