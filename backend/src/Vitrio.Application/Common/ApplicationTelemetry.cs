using System.Diagnostics;

namespace Vitrio.Application.Common;

public static class ApplicationTelemetry
{
    public const string SourceName = "Vitrio.Application";

    internal static readonly ActivitySource ActivitySource = new(SourceName);
}
