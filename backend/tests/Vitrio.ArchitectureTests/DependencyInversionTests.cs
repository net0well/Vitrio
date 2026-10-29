using Mediator;

namespace Vitrio.ArchitectureTests;

public sealed class DependencyInversionTests
{
    private static readonly Type[] HandlerInterfaces = [typeof(ICommandHandler<,>), typeof(IQueryHandler<,>)];

    [Fact]
    public void Services_HaveTheirOwnInterfaceAndHiddenImplementation()
    {
        var violations = Services()
            .Where(service => service.IsPublic || service.GetInterface($"I{service.Name}") is null)
            .Select(service => service.FullName);

        Assert.Empty(violations);
    }

    [Fact]
    public void HandlersAndServices_DependOnlyOnAbstractions()
    {
        var violations = Handlers()
            .Concat(Services())
            .SelectMany(type => type.GetConstructors().SelectMany(constructor => constructor.GetParameters()))
            .Where(parameter => !parameter.ParameterType.IsInterface)
            .Select(parameter => $"{parameter.Member.DeclaringType!.Name}({parameter.ParameterType.Name} {parameter.Name})");

        Assert.Empty(violations);
    }

    private static IEnumerable<Type> Services() =>
        Layers.Application.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith("Service", StringComparison.Ordinal));

    private static IEnumerable<Type> Handlers() =>
        Layers.Application.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && type.GetInterfaces().Any(contract => contract.IsGenericType
                    && HandlerInterfaces.Contains(contract.GetGenericTypeDefinition())));
}
