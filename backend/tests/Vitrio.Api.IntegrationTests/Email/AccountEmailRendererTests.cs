using System.Net;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Infrastructure.Email;

namespace Vitrio.Api.IntegrationTests.Email;

public sealed class AccountEmailRendererTests
{
    private readonly AccountEmailRenderer _renderer = new();

    public static TheoryData<AccountEmail> Emails() =>
    [
        new VerificationCodeEmail("ana@exemplo.com.br", "Ana", "482917"),
        new RegistrationAttemptEmail("ana@exemplo.com.br", "Ana"),
        new PasswordResetCodeEmail("ana@exemplo.com.br", "Ana", "482917"),
        new PasswordChangedEmail("ana@exemplo.com.br", "Ana", new DateTimeOffset(2026, 10, 29, 17, 5, 0, TimeSpan.Zero))
    ];

    [Theory]
    [MemberData(nameof(Emails))]
    public void Render_FillsEveryPlaceholderWithoutLinks(AccountEmail email)
    {
        var rendered = _renderer.Render(email);

        Assert.DoesNotContain("{{", rendered.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("{{", rendered.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("href", rendered.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http", rendered.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"<title>{WebUtility.HtmlEncode(rendered.Subject)}</title>", rendered.Html, StringComparison.Ordinal);
        Assert.Contains($"cid:{AccountEmailRenderer.LogoContentId}", rendered.Html, StringComparison.Ordinal);
        Assert.Contains("Olá, Ana.", rendered.Text, StringComparison.Ordinal);
        Assert.Contains("O Vitrio nunca pede sua senha", rendered.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_VerificationCode_ShowsTheCodeInSubjectAndBody()
    {
        var rendered = _renderer.Render(new VerificationCodeEmail("ana@exemplo.com.br", "Ana", "482917"));

        Assert.Equal("482917 é seu código do Vitrio", rendered.Subject);
        Assert.Contains(">482917</td>", rendered.Html, StringComparison.Ordinal);
        Assert.Contains("    482917", rendered.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_PasswordChanged_ShowsTheMomentInSaoPauloTime()
    {
        var rendered = _renderer.Render(new PasswordChangedEmail("ana@exemplo.com.br", "Ana", new DateTimeOffset(2026, 10, 29, 17, 5, 0, TimeSpan.Zero)));

        Assert.Equal("Sua senha do Vitrio foi alterada", rendered.Subject);
        Assert.Contains("foi alterada em qui 29/10 às 14:05,", rendered.Text, StringComparison.Ordinal);
        Assert.Contains("Não foi você?", rendered.Html, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_PasswordResetCode_ShowsTheCodeInSubjectAndBody()
    {
        var rendered = _renderer.Render(new PasswordResetCodeEmail("ana@exemplo.com.br", "Ana", "482917"));

        Assert.Equal("482917 é seu código para redefinir a senha", rendered.Subject);
        Assert.Contains(">482917</td>", rendered.Html, StringComparison.Ordinal);
        Assert.Contains("Redefina sua senha", rendered.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_EscapesTheNameInHtml()
    {
        var rendered = _renderer.Render(new RegistrationAttemptEmail("ana@exemplo.com.br", "<b>Ana</b>"));

        Assert.Contains("Olá, &lt;b&gt;Ana&lt;/b&gt;.", rendered.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("<b>Ana", rendered.Html, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_DoesNotReplacePlaceholdersWrittenInsideValues()
    {
        var rendered = _renderer.Render(new VerificationCodeEmail("ana@exemplo.com.br", "{{code}}", "482917"));

        Assert.Contains("Olá, {{code}}.", rendered.Text, StringComparison.Ordinal);
    }
}
