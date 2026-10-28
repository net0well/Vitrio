using System.Diagnostics.Metrics;

namespace Vitrio.Infrastructure.Events;

public sealed class OutboxMetrics
{
    public const string MeterName = "Vitrio";

    private readonly Histogram<double> _handlerDuration;
    private long _pending;
    private long _failed;
    private double _oldestPendingSeconds;

    public OutboxMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _handlerDuration = meter.CreateHistogram<double>("outbox.handler.duration", unit: "ms");
        meter.CreateObservableGauge("outbox.pending", () => Interlocked.Read(ref _pending));
        meter.CreateObservableGauge("outbox.failed", () => Interlocked.Read(ref _failed));
        meter.CreateObservableGauge("outbox.oldest_pending_seconds", () => Volatile.Read(ref _oldestPendingSeconds), unit: "s");
    }

    public void RecordHandlerDuration(string handler, TimeSpan elapsed) =>
        _handlerDuration.Record(elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("handler", handler));

    public void UpdateBacklog(long pending, long failed, double oldestPendingSeconds)
    {
        Interlocked.Exchange(ref _pending, pending);
        Interlocked.Exchange(ref _failed, failed);
        Volatile.Write(ref _oldestPendingSeconds, oldestPendingSeconds);
    }
}
