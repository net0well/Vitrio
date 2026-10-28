using Serilog;
using Serilog.Core;
using Serilog.Events;
using Vitrio.Api.Logging;

namespace Vitrio.Api.IntegrationTests.Logging;

public sealed class SensitiveDataMaskingEnricherTests
{
    [Fact]
    public void Enrich_MasksSensitivePropertiesAndKeepsTheOthers()
    {
        var logEvent = Capture(logger => logger.Information(
            "Order {OrderId} created for {Email} at {Latitude}",
            Guid.Empty,
            "ana@example.com",
            -21.25));

        Assert.Equal("***", Scalar(logEvent, "Email"));
        Assert.Equal("***", Scalar(logEvent, "Latitude"));
        Assert.Equal(Guid.Empty, Scalar(logEvent, "OrderId"));
    }

    [Fact]
    public void Enrich_MasksSensitivePropertiesInsideDestructuredObjects()
    {
        var logEvent = Capture(logger => logger.Information(
            "Customer {@Customer}",
            new { Id = 7, Phone = "+5516999990000", Addresses = new[] { new { Street = "Rua A", City = "Jaboticabal" } } }));

        var customer = Assert.IsType<StructureValue>(logEvent.Properties["Customer"]);
        Assert.Equal("***", ((ScalarValue)customer.Properties.Single(p => p.Name == "Phone").Value).Value);
        Assert.Equal(7, ((ScalarValue)customer.Properties.Single(p => p.Name == "Id").Value).Value);

        var address = Assert.IsType<StructureValue>(
            Assert.IsType<SequenceValue>(customer.Properties.Single(p => p.Name == "Addresses").Value).Elements[0]);
        Assert.Equal("***", ((ScalarValue)address.Properties.Single(p => p.Name == "Street").Value).Value);
        Assert.Equal("Jaboticabal", ((ScalarValue)address.Properties.Single(p => p.Name == "City").Value).Value);
    }

    private static LogEvent Capture(Action<ILogger> write)
    {
        var sink = new CaptureSink();
        using var logger = new LoggerConfiguration()
            .Enrich.With<SensitiveDataMaskingEnricher>()
            .WriteTo.Sink(sink)
            .CreateLogger();

        write(logger);
        return Assert.Single(sink.Events);
    }

    private static object? Scalar(LogEvent logEvent, string name) =>
        Assert.IsType<ScalarValue>(logEvent.Properties[name]).Value;

    private sealed class CaptureSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
