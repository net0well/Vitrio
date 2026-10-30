namespace Vitrio.Application.Providers.Common;

public static class ProviderCacheTags
{
    public const string Categories = "categories";

    public static string Provider(Guid providerId) => $"provider:{providerId}";
}
