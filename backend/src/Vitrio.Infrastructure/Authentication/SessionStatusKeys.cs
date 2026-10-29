namespace Vitrio.Infrastructure.Authentication;

internal static class SessionStatusKeys
{
    public static string RevokedFlag(Guid sessionId) => $"session:{sessionId}:revoked";
}
