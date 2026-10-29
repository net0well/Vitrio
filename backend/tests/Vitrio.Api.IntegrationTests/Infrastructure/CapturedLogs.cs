using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class CapturedLogs : ILogEventSink
{
    private readonly ConcurrentQueue<string> _events = new();

    public IEnumerable<string> Rendered => _events;

    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        logEvent.RenderMessage(writer);
        foreach (var property in logEvent.Properties)
        {
            writer.Write($" {property.Key}={property.Value}");
        }

        _events.Enqueue(writer.ToString());
    }
}
