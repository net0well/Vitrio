using StackExchange.Redis;

namespace Vitrio.Api.Hosting;

public static class ProductionSettings
{
    public static void EnsureComplete(IConfiguration configuration)
    {
        var problems = FindProblems(configuration);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException(
                "Production configuration is incomplete: " + string.Join(" ", problems));
        }
    }

    public static IReadOnlyList<string> FindProblems(IConfiguration configuration)
    {
        var problems = new List<string>();

        var allowedHosts = configuration["AllowedHosts"];
        if (string.IsNullOrWhiteSpace(allowedHosts) || allowedHosts.Split(';').Any(host => host.Trim() == "*"))
        {
            problems.Add("AllowedHosts must list the API host names instead of '*'.");
        }

        if (ReadList(configuration, "ForwardedHeaders:KnownProxies").Count == 0
            && ReadList(configuration, "ForwardedHeaders:KnownNetworks").Count == 0)
        {
            problems.Add(
                "ForwardedHeaders:KnownProxies or ForwardedHeaders:KnownNetworks must include the reverse proxy; " +
                "without it every client shares the proxy address in the rate limiter.");
        }

        var redis = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redis))
        {
            problems.Add("ConnectionStrings:Redis must be configured.");
        }
        else if (string.IsNullOrEmpty(ConfigurationOptions.Parse(redis).Password))
        {
            problems.Add("ConnectionStrings:Redis must include a password.");
        }

        RequireValue(configuration, "Jwt:SigningKey", "an ECDSA P-256 private key in PEM", problems);
        RequireValue(configuration, "Auth:CodeKey", "a random key of at least 32 bytes in base64", problems);
        RequireValue(configuration, "Email:Smtp:Host", "the SMTP server", problems);

        return problems;
    }

    public static IReadOnlyList<string> ReadList(IConfiguration configuration, string key) =>
        (configuration.GetSection(key).Get<string?[]>() ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .ToList();

    private static void RequireValue(IConfiguration configuration, string key, string description, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(configuration[key]))
        {
            problems.Add($"{key} must be set to {description}.");
        }
    }
}
