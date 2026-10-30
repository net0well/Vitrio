using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Infrastructure.Email;

internal sealed partial class SmtpEmailSender(
    IOptions<EmailOptions> options,
    IAccountEmailRenderer renderer,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task<Result> SendAsync(AccountEmail email, CancellationToken cancellationToken)
    {
        var kind = email.GetType().Name;
        using var message = BuildMessage(options.Value, email);
        using var client = new SmtpClient { Timeout = (int)options.Value.Smtp.Timeout.TotalMilliseconds };
        try
        {
            var smtp = options.Value.Smtp;
            await client.ConnectAsync(smtp.Host, smtp.Port, ToSocketOptions(smtp.Security), cancellationToken);
            if (!string.IsNullOrEmpty(smtp.Username))
            {
                await client.AuthenticateAsync(smtp.Username, smtp.Password ?? string.Empty, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        }
        catch (Exception exception) when (IsDeliveryFailure(exception))
        {
            LogFailed(logger, kind, exception.GetType().Name);
            return AccountErrors.EmailUnavailable;
        }

        LogSent(logger, kind);
        return Result.Success();
    }

    private MimeMessage BuildMessage(EmailOptions settings, AccountEmail email)
    {
        var rendered = renderer.Render(email);
        var body = new BodyBuilder { HtmlBody = rendered.Html, TextBody = rendered.Text };
        var logo = body.LinkedResources.Add("vitrio-wordmark.png", renderer.Logo, new ContentType("image", "png"));
        logo.ContentId = AccountEmailRenderer.LogoContentId;

        var message = new MimeMessage
        {
            Subject = rendered.Subject,
            Body = body.ToMessageBody()
        };
        message.From.Add(new MailboxAddress(settings.FromName, settings.From));
        message.To.Add(MailboxAddress.Parse(email.To));
        message.Headers.Add("Auto-Submitted", "auto-generated");
        return message;
    }

    private static SecureSocketOptions ToSocketOptions(SmtpSecurity security) => security switch
    {
        SmtpSecurity.None => SecureSocketOptions.None,
        SmtpSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
        _ => SecureSocketOptions.StartTls
    };

    private static bool IsDeliveryFailure(Exception exception) =>
        exception is IOException
            or SocketException
            or TimeoutException
            or ProtocolException
            or CommandException
            or MailKit.Security.AuthenticationException
            or ServiceNotConnectedException
            or ServiceNotAuthenticatedException;

    [LoggerMessage(Level = LogLevel.Information, Message = "Sent {EmailKind} e-mail")]
    private static partial void LogSent(ILogger logger, string emailKind);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not send {EmailKind} e-mail: {FailureType}")]
    private static partial void LogFailed(ILogger logger, string emailKind, string failureType);
}
