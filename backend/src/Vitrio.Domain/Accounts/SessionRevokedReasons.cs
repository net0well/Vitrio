namespace Vitrio.Domain.Accounts;

public static class SessionRevokedReasons
{
    public const string SignedOut = "signed_out";
    public const string SignedOutEverywhere = "signed_out_everywhere";
    public const string RevokedByUser = "revoked_by_user";
    public const string RefreshReused = "refresh_reused";
}
