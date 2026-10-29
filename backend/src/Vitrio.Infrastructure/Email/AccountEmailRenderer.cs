using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vitrio.Application.Accounts.Emails;

namespace Vitrio.Infrastructure.Email;

public sealed partial class AccountEmailRenderer
{
    public const string LogoContentId = "vitrio-wordmark";

    private readonly string _htmlLayout = ReadText("layout.html");
    private readonly string _textLayout = ReadText("layout.txt");

    public byte[] Logo { get; } = ReadBytes("vitrio-wordmark.png");

    public RenderedEmail Render(AccountEmail email) => email switch
    {
        VerificationCodeEmail verification => Compose(
            $"{verification.Code} é seu código do Vitrio",
            "Use este código para confirmar seu e-mail.",
            "verification-code",
            new() { ["firstName"] = verification.FirstName, ["code"] = verification.Code }),
        RegistrationAttemptEmail attempt => Compose(
            "Alguém tentou criar uma conta com seu e-mail",
            "Sua conta continua segura.",
            "registration-attempt",
            new() { ["firstName"] = attempt.FirstName }),
        _ => throw new ArgumentOutOfRangeException(nameof(email), email.GetType().Name, "There is no template for this e-mail.")
    };

    private RenderedEmail Compose(string subject, string preheader, string template, Dictionary<string, string> values)
    {
        var htmlValues = values.ToDictionary(pair => pair.Key, pair => WebUtility.HtmlEncode(pair.Value), StringComparer.Ordinal);
        var html = Fill(_htmlLayout, new(StringComparer.Ordinal)
        {
            ["subject"] = WebUtility.HtmlEncode(subject),
            ["preheader"] = WebUtility.HtmlEncode(preheader),
            ["content"] = Fill(ReadText($"{template}.html"), htmlValues)
        });
        var text = Fill(_textLayout, new(StringComparer.Ordinal) { ["content"] = Fill(ReadText($"{template}.txt"), values) });
        return new RenderedEmail(subject, html, text);
    }

    private static string Fill(string template, Dictionary<string, string> values) =>
        Placeholder().Replace(template, match => values.TryGetValue(match.Groups[1].Value, out var value)
            ? value
            : throw new InvalidOperationException($"The e-mail template has no value for '{match.Groups[1].Value}'."));

    private static string ReadText(string name) =>
        Encoding.UTF8.GetString(ReadBytes(name)).ReplaceLineEndings("\n").TrimEnd('\n');

    private static byte[] ReadBytes(string name)
    {
        var resource = $"Vitrio.Email.{name}";
        using var stream = typeof(AccountEmailRenderer).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Embedded resource '{resource}' was not found.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex Placeholder();
}
