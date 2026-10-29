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

        Assert.Equal(["primeiro", "segundo"], aggregate.DomainEvents.Cast<SampleRenamed>().Select(e => e.Name));
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var aggregate = new SampleAggregate();
        aggregate.Rename("primeiro");

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [EventType("sample.renamed", 1)]
    private sealed record SampleRenamed(string Name) : DomainEvent;

    private sealed class SampleAggregate : AggregateRoot
    {
        public void Rename(string name) => Raise(new SampleRenamed(name));
    }
}
