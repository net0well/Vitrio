namespace Vitrio.Application.Accounts.Emails;

public sealed record VerificationCodeEmail(string To, string FirstName, string Code) : AccountEmail(To, FirstName);
