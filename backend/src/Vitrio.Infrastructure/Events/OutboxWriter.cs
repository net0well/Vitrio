using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure.Persistence.Outbox;

namespace Vitrio.Infrastructure.Events;

public static class OutboxWriter
{
    public static OutboxMessage Append(DbContext context, DomainEvent domainEvent, DomainEventRegistry registry, DateTimeOffset now)
    {
        var eventType = domainEvent.GetType();
        var descriptor = registry.Describe(eventType);
        var message = OutboxMessage.Create(
            domainEvent.EventId,
            descriptor.Name,
            descriptor.Version,
            JsonSerializer.Serialize(domainEvent, eventType, DomainEventJson.Options),
            now);

        context.Add(message);
        foreach (var handler in registry.HandlersFor(eventType))
        {
            context.Add(OutboxDelivery.Create(message.Id, handler, now));
        }

        return message;
    }
}
