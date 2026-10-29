using Serilog.Core;
using Serilog.Events;

namespace Vitrio.Api.Logging;

// Route values can be personal data (a CEP is part of an address), so requests are logged by route template.
public sealed class RouteTemplateEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    private const string RequestPath = "RequestPath";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.ContainsKey(RequestPath)
            && httpContextAccessor.HttpContext?.GetEndpoint() is RouteEndpoint { RoutePattern.RawText: { } template })
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty(RequestPath, new ScalarValue("/" + template.TrimStart('/'))));
        }
    }
}
