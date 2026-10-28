using NetArchTest.Rules;

namespace Vitrio.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_DependsOnNoOtherLayerOrFramework()
    {
        var result = Types.InAssembly(Layers.Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Vitrio.Application",
                "Vitrio.Infrastructure",
                "Vitrio.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Mediator",
                "FluentValidation",
                "Npgsql")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrWeb()
    {
        var result = Types.InAssembly(Layers.Application)
            .ShouldNot()
            .HaveDependencyOnAny("Vitrio.Infrastructure", "Vitrio.Api", "Microsoft.AspNetCore", "Npgsql")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        var result = Types.InAssembly(Layers.Infrastructure)
            .ShouldNot()
            .HaveDependencyOn("Vitrio.Api")
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertSuccessful(NetArchTest.Rules.TestResult result) =>
        Assert.True(result.IsSuccessful, $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
