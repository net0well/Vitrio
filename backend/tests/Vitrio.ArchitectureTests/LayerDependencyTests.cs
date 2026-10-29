using NetArchTest.Rules;

namespace Vitrio.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Shared_DependsOnNoLayerOrFramework()
    {
        var result = Types.InAssembly(Layers.Shared)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Vitrio.Domain",
                "Vitrio.Application",
                "Vitrio.Infrastructure",
                "Vitrio.Jobs",
                "Vitrio.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Microsoft.Extensions",
                "Mediator",
                "FluentValidation",
                "Hangfire",
                "Npgsql")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Domain_DependsOnNoOtherLayerOrFramework()
    {
        var result = Types.InAssembly(Layers.Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Vitrio.Application",
                "Vitrio.Infrastructure",
                "Vitrio.Jobs",
                "Vitrio.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Mediator",
                "FluentValidation",
                "Hangfire",
                "Npgsql")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrWeb()
    {
        var result = Types.InAssembly(Layers.Application)
            .ShouldNot()
            .HaveDependencyOnAny("Vitrio.Infrastructure", "Vitrio.Jobs", "Vitrio.Api", "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Npgsql", "Hangfire")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Api_DoesNotTouchThePersistenceLayer()
    {
        var result = Types.InAssembly(Layers.Api)
            .ShouldNot()
            .HaveDependencyOnAny("Vitrio.Infrastructure.Persistence", "Microsoft.EntityFrameworkCore")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Api_UsesInfrastructureOnlyInTheComposition()
    {
        var result = Types.InAssembly(Layers.Api)
            .That()
            .ResideInNamespaceStartingWith("Vitrio.Api")
            .ShouldNot()
            .HaveDependencyOn("Vitrio.Infrastructure")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApiOrJobs()
    {
        var result = Types.InAssembly(Layers.Infrastructure)
            .ShouldNot()
            .HaveDependencyOnAny("Vitrio.Api", "Vitrio.Jobs", "Hangfire")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void NoLayer_UsesMappingLibraries()
    {
        var result = Types.InAssemblies([Layers.Shared, Layers.Domain, Layers.Application, Layers.Infrastructure, Layers.Api])
            .ShouldNot()
            .HaveDependencyOnAny("AutoMapper", "Mapster", "Riok.Mapperly", "AgileObjects.AgileMapper", "ExpressMapper", "Nelibur.ObjectMapper")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void NoLayer_UsesMediatR()
    {
        var result = Types.InAssemblies([Layers.Shared, Layers.Domain, Layers.Application, Layers.Infrastructure, Layers.Api])
            .ShouldNot()
            .HaveDependencyOn("MediatR")
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertSuccessful(NetArchTest.Rules.TestResult result) =>
        Assert.True(result.IsSuccessful, $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
