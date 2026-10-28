using Vitrio.Application.Events;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure.Events;

namespace Vitrio.ArchitectureTests;

public sealed class DomainEventTests
{
    private static readonly Type[] ProductionTypes =
        [.. new[] { Layers.Domain, Layers.Application, Layers.Infrastructure }.SelectMany(assembly => assembly.GetTypes())];

    [Fact]
    public void EveryDomainEvent_HasAUniqueStableName()
    {
        var exception = Record.Exception(() => new DomainEventRegistry(ProductionTypes));

        Assert.Null(exception);
    }

    [Fact]
    public void DomainEvents_LiveInTheDomainLayer()
    {
        var misplaced = ProductionTypes
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsAssignableTo(typeof(DomainEvent)))
            .Where(type => type.Assembly != Layers.Domain)
            .Select(type => type.FullName);

        Assert.Empty(misplaced);
    }

    [Fact]
    public void EventHandlers_AreSealed()
    {
        var unsealed = ProductionTypes
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.GetInterfaces().Any(contract =>
                contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
            .Where(type => !type.IsSealed)
            .Select(type => type.FullName);

        Assert.Empty(unsealed);
    }
}
