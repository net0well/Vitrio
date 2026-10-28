using Microsoft.Extensions.Configuration;
using Vitrio.Api.Hosting;

namespace Vitrio.Api.IntegrationTests.Hosting;

public sealed class ProductionSettingsTests
{
    private static readonly Dictionary<string, string?> CompleteSettings = new()
    {
        ["AllowedHosts"] = "api.vitrio.app",
        ["ForwardedHeaders:KnownNetworks:0"] = "172.30.0.0/24",
        ["ConnectionStrings:Redis"] = "redis:6379,password=secret-value"
    };

    [Fact]
    public void FindProblems_WithCompleteSettings_ReturnsNothing()
    {
        Assert.Empty(ProductionSettings.FindProblems(Build(CompleteSettings)));
    }

    [Fact]
    public void FindProblems_WithWildcardHost_ReportsAllowedHosts()
    {
        var problems = ProductionSettings.FindProblems(Build(CompleteSettings, ("AllowedHosts", "*")));

        Assert.Contains(problems, problem => problem.Contains("AllowedHosts", StringComparison.Ordinal));
    }

    [Fact]
    public void FindProblems_WithoutKnownProxy_ReportsForwardedHeaders()
    {
        var problems = ProductionSettings.FindProblems(Build(CompleteSettings, ("ForwardedHeaders:KnownNetworks:0", null)));

        Assert.Contains(problems, problem => problem.Contains("ForwardedHeaders", StringComparison.Ordinal));
    }

    [Fact]
    public void FindProblems_WithRedisWithoutPassword_ReportsRedis()
    {
        var problems = ProductionSettings.FindProblems(Build(CompleteSettings, ("ConnectionStrings:Redis", "redis:6379")));

        Assert.Contains(problems, problem => problem.Contains("password", StringComparison.Ordinal));
    }

    [Fact]
    public void EnsureComplete_WithMissingSettings_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => ProductionSettings.EnsureComplete(Build([])));
    }

    private static IConfiguration Build(Dictionary<string, string?> settings, params (string Key, string? Value)[] overrides)
    {
        var merged = new Dictionary<string, string?>(settings);
        foreach (var (key, value) in overrides)
        {
            merged[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(merged).Build();
    }
}
