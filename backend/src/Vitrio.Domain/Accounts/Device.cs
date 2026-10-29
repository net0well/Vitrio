using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public sealed class Device
{
    public const int InstallationIdMaxLength = 64;
    public const int PushTokenMaxLength = 4096;

    private Device()
    {
    }

    public string InstallationId { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }

    public DevicePlatform Platform { get; private set; }

    public string PushToken { get; private set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; private set; }

    public static bool IsValidInstallationId(string? installationId) =>
        installationId is { Length: >= 8 and <= InstallationIdMaxLength }
        && installationId.All(character => char.IsAsciiLetterOrDigit(character) || character == '-');

    public static Device Register(string installationId, Guid userId, DevicePlatform platform, string pushToken, IClock clock)
    {
        var device = new Device { InstallationId = installationId };
        device.Assign(userId, platform, pushToken, clock);
        return device;
    }

    public void Assign(Guid userId, DevicePlatform platform, string pushToken, IClock clock)
    {
        UserId = userId;
        Platform = platform;
        PushToken = pushToken;
        UpdatedAt = clock.UtcNow;
    }
}
