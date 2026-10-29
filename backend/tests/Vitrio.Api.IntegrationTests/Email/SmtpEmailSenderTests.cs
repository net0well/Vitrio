using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Accounts;
using Vitrio.Infrastructure.Email;

namespace Vitrio.Api.IntegrationTests.Email;

public sealed class SmtpEmailSenderTests : IAsyncLifetime
{
    private const int SmtpPort = 1025;
    private const int ApiPort = 8025;
    private const double MinimumHtmlSupport = 85;

    private readonly IContainer _mailpit = new ContainerBuilder("axllent/mailpit:v1.32")
        .WithPortBinding(SmtpPort, assignRandomHostPort: true)
        .WithPortBinding(ApiPort, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request.ForPort(ApiPort).ForPath("/livez")))
        .Build();

    private HttpClient _mailbox = null!;

    public async ValueTask InitializeAsync()
    {
        await _mailpit.StartAsync(TestContext.Current.CancellationToken);
        _mailbox = new HttpClient { BaseAddress = new Uri($"http://{_mailpit.Hostname}:{_mailpit.GetMappedPublicPort(ApiPort)}") };
    }

    public async ValueTask DisposeAsync()
    {
        _mailbox.Dispose();
        await _mailpit.DisposeAsync();
    }

    [Fact]
    public async Task SendAsync_VerificationCode_DeliversHtmlAndTextWithTheEmbeddedLogo()
    {
        var sender = CreateSender(_mailpit.Hostname, _mailpit.GetMappedPublicPort(SmtpPort));

        var result = await sender.SendAsync(new VerificationCodeEmail("ana@exemplo.com.br", "Ana", "482917"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var message = await ReadMessageAsync("ana@exemplo.com.br");
        Assert.Equal("482917 é seu código do Vitrio", message.GetProperty("Subject").GetString());
        Assert.Equal("nao-responda@vitrio.app", message.GetProperty("From").GetProperty("Address").GetString());
        Assert.Equal("Vitrio", message.GetProperty("From").GetProperty("Name").GetString());
        Assert.Contains("482917", message.GetProperty("HTML").GetString(), StringComparison.Ordinal);
        Assert.Contains("482917", message.GetProperty("Text").GetString(), StringComparison.Ordinal);
        Assert.DoesNotContain("href", message.GetProperty("HTML").GetString(), StringComparison.OrdinalIgnoreCase);

        var logo = Assert.Single(message.GetProperty("Inline").EnumerateArray());
        Assert.Equal(AccountEmailRenderer.LogoContentId, logo.GetProperty("ContentID").GetString());
        Assert.Equal("image/png", logo.GetProperty("ContentType").GetString());
        Assert.Empty(message.GetProperty("Attachments").EnumerateArray());

        var id = message.GetProperty("ID").GetString();
        var headers = await _mailbox.GetFromJsonAsync<JsonElement>($"/api/v1/message/{id}/headers", TestContext.Current.CancellationToken);
        Assert.Equal("auto-generated", headers.GetProperty("Auto-Submitted")[0].GetString());
        Assert.False(headers.TryGetProperty("List-Unsubscribe", out _));

        var htmlCheck = await _mailbox.GetFromJsonAsync<JsonElement>($"/api/v1/message/{id}/html-check", TestContext.Current.CancellationToken);
        Assert.True(htmlCheck.GetProperty("Total").GetProperty("Supported").GetDouble() >= MinimumHtmlSupport);
    }

    [Fact]
    public async Task SendAsync_RegistrationAttempt_DeliversTheWarningWithoutCode()
    {
        var sender = CreateSender(_mailpit.Hostname, _mailpit.GetMappedPublicPort(SmtpPort));

        var result = await sender.SendAsync(new RegistrationAttemptEmail("bruno@exemplo.com.br", "Bruno"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var message = await ReadMessageAsync("bruno@exemplo.com.br");
        Assert.Equal("Alguém tentou criar uma conta com seu e-mail", message.GetProperty("Subject").GetString());
        Assert.Contains("Esqueceu a senha?", message.GetProperty("Text").GetString(), StringComparison.Ordinal);
        Assert.DoesNotMatch("[0-9]{6}", message.GetProperty("Text").GetString());
    }

    [Fact]
    public async Task SendAsync_WhenTheServerIsUnreachable_ReturnsEmailUnavailable()
    {
        var sender = CreateSender(IPAddress.Loopback.ToString(), ClosedPort());

        var result = await sender.SendAsync(new VerificationCodeEmail("ana@exemplo.com.br", "Ana", "482917"), TestContext.Current.CancellationToken);

        Assert.Equal(AccountErrors.EmailUnavailable, result.Error);
    }

    private static SmtpEmailSender CreateSender(string host, int port) => new(
        Options.Create(new EmailOptions
        {
            From = "nao-responda@vitrio.app",
            FromName = "Vitrio",
            Smtp = new SmtpOptions { Host = host, Port = port, Security = SmtpSecurity.None, Timeout = TimeSpan.FromSeconds(5) }
        }),
        new AccountEmailRenderer(),
        NullLogger<SmtpEmailSender>.Instance);

    private static int ClosedPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private async Task<JsonElement> ReadMessageAsync(string recipient)
    {
        var search = await _mailbox.GetFromJsonAsync<JsonElement>(
            $"/api/v1/search?query={Uri.EscapeDataString($"to:{recipient}")}",
            TestContext.Current.CancellationToken);
        var id = Assert.Single(search.GetProperty("messages").EnumerateArray()).GetProperty("ID").GetString();
        return await _mailbox.GetFromJsonAsync<JsonElement>($"/api/v1/message/{id}", TestContext.Current.CancellationToken);
    }
}
