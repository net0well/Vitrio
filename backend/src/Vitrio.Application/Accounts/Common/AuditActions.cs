namespace Vitrio.Application.Accounts.Common;

public static class AuditActions
{
    public const string AccountRegistered = "account.registered";
    public const string EmailVerified = "account.email_verified";
    public const string SignedIn = "account.signed_in";
    public const string SignInLockedOut = "account.sign_in_locked_out";
    public const string SignedOut = "account.signed_out";
    public const string SignedOutEverywhere = "account.signed_out_everywhere";
    public const string SessionRevoked = "account.session_revoked";
    public const string RefreshReused = "account.refresh_reused";
    public const string PasswordReset = "account.password_reset";
    public const string PasswordChanged = "account.password_changed";
    public const string AccountDeleted = "account.deleted";
    public const string AdminGranted = "account.admin_granted";
}
