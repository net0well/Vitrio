using Vitrio.Application.Events;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure.Events;

namespace Vitrio.Api.IntegrationTests.Events;

public sealed class DomainEventRegistryTests
{
    [Fact]
    public void Constructor_MapsStableNamesAndHandlers()
    {
        var registry = new DomainEventRegistry([typeof(QuoteWasSent), typeof(QuoteNotifier)]);

        Assert.Equal("tests.quote_sent", registry.Describe(typeof(QuoteWasSent)).Name);
        Assert.Equal(typeof(QuoteWasSent), registry.EventType("tests.quote_sent", 2));
        Assert.Equal([nameof(QuoteNotifier)], registry.HandlersFor(typeof(QuoteWasSent)));
        Assert.Equal(typeof(QuoteNotifier), registry.HandlerType(nameof(QuoteNotifier)));
    }

    [Fact]
    public void Constructor_WithEventWithoutEventType_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => new DomainEventRegistry([typeof(Unnamed)]));

        Assert.Contains(nameof(Unnamed), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WithRepeatedStableName_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new DomainEventRegistry([typeof(QuoteWasSent), typeof(QuoteWasSentAgain)]));
    }

    [Fact]
    public void HandlersFor_EventWithoutHandlers_IsEmpty()
    {
        var registry = new DomainEventRegistry([typeof(QuoteWasSent)]);

        Assert.Empty(registry.HandlersFor(typeof(QuoteWasSent)));
    }

    [EventType("tests.quote_sent", 2)]
    private sealed record QuoteWasSent(Guid OrderId) : DomainEvent;

    [EventType("tests.quote_sent", 2)]
    private sealed record QuoteWasSentAgain(Guid OrderId) : DomainEvent;

    private sealed record Unnamed(Guid Id) : DomainEvent;

    private sealed class QuoteNotifier : IDomainEventHandler<QuoteWasSent>
    {
        public ValueTask Handle(QuoteWasSent domainEvent, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }
}
