using Vitrio.Domain.Common;

namespace Vitrio.Domain.Tests.Common;

public sealed class AggregateRootTests
{
    [Fact]
    public void Raise_KeepsEventsInOrder()
    {
        var aggregate = new SampleAggregate();

        aggregate.Rename("primeiro");
        aggregate.Rename("segundo");

        Assert.Equal(
            [new SampleRenamed("primeiro"), new SampleRenamed("segundo")],
            aggregate.DomainEvents.Cast<SampleRenamed>());
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var aggregate = new SampleAggregate();
        aggregate.Rename("primeiro");

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    private sealed record SampleRenamed(string Name) : IDomainEvent;

    private sealed class SampleAggregate : AggregateRoot
    {
        public void Rename(string name) => Raise(new SampleRenamed(name));
    }
}
