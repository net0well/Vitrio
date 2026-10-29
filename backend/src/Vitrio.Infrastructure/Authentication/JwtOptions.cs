using System.ComponentModel.DataAnnotations;

namespace Vitrio.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    public string? SigningKey { get; init; }

    [Range(typeof(TimeSpan), "00:01:00", "01:00:00")]
    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(10);
}
