namespace Vitrio.Infrastructure.Events;

public sealed class DomainEventOptions
{
    public HashSet<Type> Types { get; } = [];
}
