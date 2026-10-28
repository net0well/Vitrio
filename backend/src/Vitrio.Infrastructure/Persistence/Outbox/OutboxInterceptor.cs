using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Vitrio.Domain.Common;

namespace Vitrio.Infrastructure.Persistence.Outbox;

internal sealed class OutboxInterceptor(IClock clock) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            MoveDomainEventsToOutbox(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            MoveDomainEventsToOutbox(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void MoveDomainEventsToOutbox(DbContext context)
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        if (aggregates.Count == 0)
        {
            return;
        }

        var occurredAt = clock.UtcNow;
        var messages = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .Select(domainEvent => OutboxMessage.Create(
                domainEvent.GetType().FullName!,
                JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions),
                occurredAt))
            .ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        context.Set<OutboxMessage>().AddRange(messages);
    }
}
