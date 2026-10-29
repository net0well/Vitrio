namespace Vitrio.Infrastructure.Accounts;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string? CodeKey { get; init; }
}
