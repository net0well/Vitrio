namespace Vitrio.Domain.Common;

public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
}
