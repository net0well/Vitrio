using Mediator;
using NetArchTest.Rules;
using Vitrio.Application.Abstractions;

namespace Vitrio.ArchitectureTests;

public sealed class PersistenceConventionTests
{
    [Fact]
    public void RepositoryInterfaces_AreWritableOrSearchablePerAggregate()
    {
        var violations = RepositoryInterfaces()
            .Where(type => type.IsGenericType
                || type.Namespace?.EndsWith(".Repositories", StringComparison.Ordinal) != true
                || !(type.Name.EndsWith("WritableRepository", StringComparison.Ordinal)
                    || type.Name.EndsWith("SearchableRepository", StringComparison.Ordinal)))
            .Select(type => type.FullName);

        Assert.Empty(violations);
    }

    [Fact]
    public void RepositoryInterfaces_DoNotExposeQueryables()
    {
        var violations = RepositoryInterfaces()
            .SelectMany(type => type.GetMethods())
            .Where(method => ExposesQueryable(method.ReturnType))
            .Select(method => $"{method.DeclaringType!.Name}.{method.Name}");

        Assert.Empty(violations);
    }

    [Fact]
    public void RepositoryInterfaces_AreImplementedInInfrastructure()
    {
        var implemented = Layers.Infrastructure.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetInterfaces())
            .ToHashSet();

        var violations = RepositoryInterfaces()
            .Where(type => !implemented.Contains(type))
            .Select(type => type.FullName);

        Assert.Empty(violations);
    }

    [Fact]
    public void Services_NeitherSaveNorSendMessages()
    {
        var result = Types.InAssembly(Layers.Application)
            .That()
            .HaveNameEndingWith("Service")
            .ShouldNot()
            .HaveDependencyOnAny(typeof(IUnitOfWork).FullName!, typeof(ISender).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    private static IEnumerable<Type> RepositoryInterfaces() =>
        Layers.Application.GetTypes()
            .Where(type => type.IsInterface && type.Name.EndsWith("Repository", StringComparison.Ordinal));

    private static bool ExposesQueryable(Type type) =>
        typeof(IQueryable).IsAssignableFrom(type) || (type.IsGenericType && type.GetGenericArguments().Any(ExposesQueryable));
}
