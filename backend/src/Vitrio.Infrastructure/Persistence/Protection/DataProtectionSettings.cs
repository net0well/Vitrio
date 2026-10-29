namespace Vitrio.Infrastructure.Persistence.Protection;

public sealed class DataProtectionSettings
{
    public const string SectionName = "DataProtection";

    public string? Certificate { get; init; }

    public string? CertificatePassword { get; init; }
}
