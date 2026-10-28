using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Infrastructure.Persistence.Outbox;
using Vitrio.Infrastructure.Time;

namespace Vitrio.Api.IntegrationTests.Persistence;

public sealed class OutboxInterceptorTests(ApiFactory factory) : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly string _connectionString = new NpgsqlConnectionStringBuilder(factory.ConnectionString)
    {
        Database = "outbox_tests"
    }.ConnectionString;

    public async ValueTask InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SaveChanges_StoresDomainEventsInTheOutboxAndClearsThem()
    {
        var aggregate = SampleAggregate.Create("Pintura");
        await using (var context = CreateContext())
        {
            context.Samples.Add(aggregate);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Assert.Empty(aggregate.DomainEvents);

        await using var verification = CreateContext();
        var message = await verification.OutboxMessages.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(typeof(SampleCreated).FullName, message.Type);
        Assert.Equal(Now, message.OccurredAt);
        Assert.Null(message.ProcessedAt);
        var content = JsonSerializer.Deserialize<JsonElement>(message.Content);
        Assert.Equal(aggregate.Id, content.GetProperty("sampleId").GetGuid());
    }

    [Fact]
    public async Task SaveChanges_WithoutDomainEvents_DoesNotWriteToTheOutbox()
    {
        await using (var context = CreateContext())
        {
            context.Samples.Add(SampleAggregate.CreateSilently("Elétrica"));
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var verification = CreateContext();
        Assert.Equal(0, await verification.OutboxMessages.CountAsync(TestContext.Current.CancellationToken));
    }

    private SampleDbContext CreateContext()
    {
        var clock = new SystemClock(new FakeTimeProvider(Now));
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseVitrioDatabase(_connectionString)
            .AddInterceptors(new OutboxInterceptor(clock))
            .Options;

        return new SampleDbContext(options);
    }

    private sealed record SampleCreated(Guid SampleId) : IDomainEvent;

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

        public static SampleAggregate CreateSilently(string name) => new() { Id = Guid.CreateVersion7(), Name = name };
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
