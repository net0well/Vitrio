using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vitrio.Application.Events;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Infrastructure.Persistence.Outbox;

namespace Vitrio.Infrastructure.Events;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    DomainEventRegistry registry,
    IClock clock,
    IOptions<OutboxOptions> options,
    OutboxMetrics metrics,
    ILogger<OutboxProcessor> logger)
{
    private const string ReserveSql = """
        update outbox_deliveries d
        set locked_until = @locked_until,
            attempts     = d.attempts + 1
        where (d.message_id, d.handler) in (
          select message_id, handler
          from outbox_deliveries
          where status = 'Pending'
            and next_attempt_at <= @now
            and (locked_until is null or locked_until < @now)
          order by next_attempt_at
          limit @batch_size
          for update skip locked)
        returning d.message_id, d.handler, d.attempts
        """;

    private static readonly ConcurrentDictionary<Type, Func<object, DomainEvent, CancellationToken, ValueTask>> Invokers = new();

    private static readonly MethodInfo InvokeMethod =
        typeof(OutboxProcessor).GetMethod(nameof(Invoke), BindingFlags.NonPublic | BindingFlags.Static)!;

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var reserved = await ReserveAsync(cancellationToken);
        foreach (var delivery in reserved)
        {
            await DeliverAsync(delivery, cancellationToken);
        }

        return reserved.Count;
    }

    public async Task RefreshBacklogAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = await db.OutboxDeliveries.LongCountAsync(d => d.Status == OutboxDeliveryStatus.Pending, cancellationToken);
        var failed = await db.OutboxDeliveries.LongCountAsync(d => d.Status == OutboxDeliveryStatus.Failed, cancellationToken);
        var oldest = await db.OutboxDeliveries
            .Where(d => d.Status == OutboxDeliveryStatus.Pending)
            .Join(db.OutboxMessages, d => d.MessageId, m => m.Id, (_, m) => (DateTimeOffset?)m.OccurredAt)
            .MinAsync(cancellationToken);

        metrics.UpdateBacklog(pending, failed, oldest is { } occurredAt ? (clock.UtcNow - occurredAt).TotalSeconds : 0);
    }

    private async Task<List<ReservedDelivery>> ReserveAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = clock.UtcNow;

        await db.Database.OpenConnectionAsync(cancellationToken);
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = ReserveSql;
        AddParameter(command, "now", now);
        AddParameter(command, "locked_until", now + options.Value.LockDuration);
        AddParameter(command, "batch_size", options.Value.BatchSize);

        var reserved = new List<ReservedDelivery>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            reserved.Add(new ReservedDelivery(reader.GetGuid(0), reader.GetString(1), reader.GetInt32(2)));
        }

        return reserved;
    }

    private async Task DeliverAsync(ReservedDelivery reserved, CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var eventName = "unknown";
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var message = await db.OutboxMessages.AsNoTracking().SingleAsync(m => m.Id == reserved.MessageId, cancellationToken);
            eventName = message.Type;
            var eventType = registry.EventType(message.Type, message.Version);
            var domainEvent = (DomainEvent)JsonSerializer.Deserialize(message.Payload, eventType, DomainEventJson.Options)!;
            var handler = scope.ServiceProvider.GetRequiredService(registry.HandlerType(reserved.Handler));

            await InvokerFor(eventType)(handler, domainEvent, cancellationToken);

            var delivery = await db.OutboxDeliveries.SingleAsync(
                d => d.MessageId == reserved.MessageId && d.Handler == reserved.Handler,
                cancellationToken);
            delivery.MarkSucceeded(clock.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogDebug(
                "Delivered {EventType} {MessageId} to {Handler} on attempt {Attempt}",
                eventName,
                reserved.MessageId,
                reserved.Handler,
                reserved.Attempt);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            await RegisterFailureAsync(reserved, eventName, exception, cancellationToken);
        }
        finally
        {
            metrics.RecordHandlerDuration(reserved.Handler, Stopwatch.GetElapsedTime(startedAt));
        }
    }

    private async Task RegisterFailureAsync(ReservedDelivery reserved, string eventName, Exception exception, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var delivery = await db.OutboxDeliveries.SingleAsync(
            d => d.MessageId == reserved.MessageId && d.Handler == reserved.Handler,
            cancellationToken);

        delivery.RegisterFailure(exception.GetType().Name, clock.UtcNow);
        await db.SaveChangesAsync(cancellationToken);

        if (delivery.Status == OutboxDeliveryStatus.Failed)
        {
            logger.LogError(
                "Gave up delivering {EventType} {MessageId} to {Handler} after {Attempt} attempts ({ErrorType})",
                eventName,
                reserved.MessageId,
                reserved.Handler,
                reserved.Attempt,
                exception.GetType().Name);
        }
        else
        {
            logger.LogWarning(
                "Delivery of {EventType} {MessageId} to {Handler} failed on attempt {Attempt} ({ErrorType}); retrying at {NextAttemptAt}",
                eventName,
                reserved.MessageId,
                reserved.Handler,
                reserved.Attempt,
                exception.GetType().Name,
                delivery.NextAttemptAt);
        }
    }

    private static Func<object, DomainEvent, CancellationToken, ValueTask> InvokerFor(Type eventType) =>
        Invokers.GetOrAdd(eventType, type =>
            InvokeMethod.MakeGenericMethod(type).CreateDelegate<Func<object, DomainEvent, CancellationToken, ValueTask>>());

    private static ValueTask Invoke<TEvent>(object handler, DomainEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : DomainEvent =>
        ((IDomainEventHandler<TEvent>)handler).Handle((TEvent)domainEvent, cancellationToken);

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
