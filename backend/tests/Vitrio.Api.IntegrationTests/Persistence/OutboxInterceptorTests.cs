using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Events;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure.Events;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Infrastructure.Persistence.Outbox;
using Vitrio.Infrastructure.Time;

namespace Vitrio.Api.IntegrationTests.Persistence;

public sealed class OutboxInterceptorTests(ApiFactory factory) : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly DomainEventRegistry Registry =
        new([typeof(SampleCreated), typeof(SampleNotifications), typeof(SampleCounters)]);

    private readonly string _connectionString = new NpgsqlConnectionStringBuilder(factory.ConnectionString)
    {
        Database = "outbox_interceptor_tests"
    }.ConnectionString;

    public async ValueTask InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SaveChanges_WritesTheMessageAndOneDeliveryPerHandler()
    {
        var aggregate = SampleAggregate.Create("Pintura");
        var eventId = aggregate.DomainEvents.Single().EventId;
        await using (var context = CreateContext())
        {
            context.Samples.Add(aggregate);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Assert.Empty(aggregate.DomainEvents);

        await using var verification = CreateContext();
        var message = await verification.OutboxMessages.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(eventId, message.Id);
        Assert.Equal("sample.created", message.Type);
        Assert.Equal(1, message.Version);
        Assert.Equal(Now, message.OccurredAt);
        Assert.Equal(aggregate.Id, JsonSerializer.Deserialize<JsonElement>(message.Payload).GetProperty("sampleId").GetGuid());

        var deliveries = await verification.OutboxDeliveries.AsNoTracking().OrderBy(d => d.Handler).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal([nameof(SampleCounters), nameof(SampleNotifications)], deliveries.Select(d => d.Handler));
        Assert.All(deliveries, delivery =>
        {
            Assert.Equal(OutboxDeliveryStatus.Pending, delivery.Status);
            Assert.Equal(0, delivery.Attempts);
            Assert.Equal(Now, delivery.NextAttemptAt);
        });
    }

    [Fact]
    public async Task SaveChanges_WithoutDomainEvents_WritesNothingToTheOutbox()
    {
        await using (var context = CreateContext())
        {
            context.Samples.Add(SampleAggregate.CreateSilently("Elétrica"));
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var verification = CreateContext();
        Assert.Equal(0, await verification.OutboxMessages.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await verification.OutboxDeliveries.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChanges_WhenTheTransactionFails_WritesNoEvent()
    {
        var existing = SampleAggregate.CreateSilently("Hidráulica");
        await using (var context = CreateContext())
        {
            context.Samples.Add(existing);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = CreateContext())
        {
            context.Samples.Add(SampleAggregate.Create("Pintura"));
            context.Samples.Add(SampleAggregate.CreateSilently("Duplicada", existing.Id));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
        }

        await using var verification = CreateContext();
        Assert.Equal(0, await verification.OutboxMessages.CountAsync(TestContext.Current.CancellationToken));
    }

    private SampleDbContext CreateContext()
    {
        var clock = new SystemClock(new FakeTimeProvider(Now));
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseVitrioDatabase(_connectionString)
            .AddInterceptors(new OutboxInterceptor(clock, Registry))
            .Options;

        return new SampleDbContext(options);
    }

    [EventType("sample.created", 1)]
    private sealed record SampleCreated(Guid SampleId) : DomainEvent;

    private sealed class SampleNotifications : IDomainEventHandler<SampleCreated>
    {
        public ValueTask Handle(SampleCreated domainEvent, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }

    private sealed class SampleCounters : IDomainEventHandler<SampleCreated>
    {
        public ValueTask Handle(SampleCreated domainEvent, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }

    private sealed class SampleAggregate : AggregateRoot
    {
        private SampleAggregate()
        {
        }

        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public static SampleAggregate Create(string name)
        {
            var aggregate = CreateSilently(name);
            aggregate.Raise(new SampleCreated(aggregate.Id));
            return aggregate;
        }

        public static SampleAggregate CreateSilently(string name, Guid? id = null) =>
            new() { Id = id ?? Guid.CreateVersion7(), Name = name };
    }

    private sealed class SampleDbContext(DbContextOptions options) : AppDbContext(options)
    {
        public DbSet<SampleAggregate> Samples => Set<SampleAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SampleAggregate>().ToTable("samples").Ignore(sample => sample.DomainEvents);
        }
    }
}
