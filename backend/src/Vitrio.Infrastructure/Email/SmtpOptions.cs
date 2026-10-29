namespace Vitrio.Infrastructure.Email;

public sealed class SmtpOptions
{
    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public SmtpSecurity Security { get; init; } = SmtpSecurity.StartTls;

    public string? Username { get; init; }

    public string? Password { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
}
