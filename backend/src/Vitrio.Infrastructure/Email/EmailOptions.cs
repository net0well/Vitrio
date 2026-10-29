using System.ComponentModel.DataAnnotations;

namespace Vitrio.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    [EmailAddress]
    public string From { get; init; } = string.Empty;

    [Required]
    public string FromName { get; init; } = string.Empty;

    [Required]
    public SmtpOptions Smtp { get; init; } = new();
}
