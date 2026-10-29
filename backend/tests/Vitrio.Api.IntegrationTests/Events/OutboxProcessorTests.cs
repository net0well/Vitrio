using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Events;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure;
using Vitrio.Infrastructure.Events;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Infrastructure.Persistence.Outbox;

namespace Vitrio.Api.IntegrationTests.Events;

public sealed class OutboxProcessorTests(ApiFactory factory) : IAsyncLifetime
{
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero));
    private readonly HandlerLog _log = new();
    private ServiceProvider _services = null!;

    public async ValueTask InitializeAsync()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(factory.ConnectionString) { Database = "outbox_processor_tests" }.ConnectionString;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Database"] = connectionString })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(_time);
        services.AddSingleton(_log);
        services.AddInfrastructure(configuration);
        services.AddDomainEventTypes([typeof(ProbeHappened), typeof(ProbeRecorder), typeof(FlakyProbeHandler)]);
        _services = services.BuildServiceProvider();

        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        await db.OutboxDeliveries.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
        await db.OutboxMessages.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() => await _services.DisposeAsync();

    [Fact]
    public async Task ProcessBatch_DeliversToEachHandlerIndependently()
    {
        _log.FailuresBeforeSuccess = 1;
        var probe = await AppendAsync();

        await ProcessAsync();

        Assert.Equal([probe.ProbeId], _log.Recorded);
        var deliveries = await DeliveriesAsync();
        Assert.Equal(OutboxDeliveryStatus.Succeeded, deliveries[nameof(ProbeRecorder)].Status);
        var flaky = deliveries[nameof(FlakyProbeHandler)];
        Assert.Equal(OutboxDeliveryStatus.Pending, flaky.Status);
        Assert.Equal(1, flaky.Attempts);
        Assert.Equal(nameof(InvalidOperationException), flaky.LastError);
        Assert.Equal(_time.GetUtcNow().AddSeconds(5), flaky.NextAttemptAt);
    }

    [Fact]
    public async Task ProcessBatch_RetriesOnlyTheFailedHandlerAfterTheBackoff()
    {
        _log.FailuresBeforeSuccess = 1;
        await AppendAsync();
        await ProcessAsync();

        await ProcessAsync();
        Assert.Equal(OutboxDeliveryStatus.Pending, (await DeliveriesAsync())[nameof(FlakyProbeHandler)].Status);

        _time.Advance(TimeSpan.FromSeconds(5));
        await ProcessAsync();

        var deliveries = await DeliveriesAsync();
        Assert.Equal(OutboxDeliveryStatus.Succeeded, deliveries[nameof(FlakyProbeHandler)].Status);
        Assert.Equal(2, deliveries[nameof(FlakyProbeHandler)].Attempts);
        Assert.Single(_log.Recorded);
    }

    [Fact]
    public async Task ProcessBatch_AfterTenFailures_MarksTheDeliveryAsFailed()
    {
        _log.FailuresBeforeSuccess = int.MaxValue;
        await AppendAsync();

        for (var attempt = 0; attempt < OutboxDelivery.MaxAttempts; attempt++)
        {
            await ProcessAsync();
            _time.Advance(TimeSpan.FromHours(1));
        }

        await ProcessAsync();

        var flaky = (await DeliveriesAsync())[nameof(FlakyProbeHandler)];
        Assert.Equal(OutboxDeliveryStatus.Failed, flaky.Status);
        Assert.Equal(OutboxDelivery.MaxAttempts, flaky.Attempts);
    }

    [Fact]
    public async Task ProcessBatch_RunningInParallel_DeliversEachEventOnce()
    {
        var probes = new List<ProbeHappened>();
        for (var index = 0; index < 20; index++)
        {
            probes.Add(await AppendAsync());
        }

        var processor = _services.GetRequiredService<OutboxProcessor>();
        await Task.WhenAll(
            processor.ProcessBatchAsync(TestContext.Current.CancellationToken),
            processor.ProcessBatchAsync(TestContext.Current.CancellationToken));
        await processor.ProcessBatchAsync(TestContext.Current.CancellationToken);

        Assert.Equal(probes.Select(p => p.ProbeId).Order(), _log.Recorded.Order());
    }

    private async Task<ProbeHappened> AppendAsync()
    {
        var probe = new ProbeHappened(Guid.CreateVersion7());
        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        OutboxWriter.Append(db, probe, scope.ServiceProvider.GetRequiredService<DomainEventRegistry>(), _time.GetUtcNow());
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return probe;
    }

    private Task<int> ProcessAsync() =>
        _services.GetRequiredService<OutboxProcessor>().ProcessBatchAsync(TestContext.Current.CancellationToken);

    private async Task<Dictionary<string, OutboxDelivery>> DeliveriesAsync()
    {
        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.OutboxDeliveries.AsNoTracking().ToDictionaryAsync(d => d.Handler, TestContext.Current.CancellationToken);
    }

    [EventType("tests.probe_happened", 1)]
    private sealed record ProbeHappened(Guid ProbeId) : DomainEvent;

    private sealed class HandlerLog
    {
        private int _failures;

        public ConcurrentBag<Guid> Recorded { get; } = [];

        public int FailuresBeforeSuccess { get; set; }

        public bool ShouldFail() => Interlocked.Increment(ref _failures) <= FailuresBeforeSuccess;
    }

    private sealed class ProbeRecorder(HandlerLog log) : IDomainEventHandler<ProbeHappened>
    {
        public ValueTask Handle(ProbeHappened domainEvent, CancellationToken cancellationToken)
        {
            log.Recorded.Add(domainEvent.ProbeId);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FlakyProbeHandler(HandlerLog log) : IDomainEventHandler<ProbeHappened>
    {
        public ValueTask Handle(ProbeHappened domainEvent, CancellationToken cancellationToken) =>
            log.ShouldFail() ? throw new InvalidOperationException("Simulated failure.") : ValueTask.CompletedTask;
    }
}
