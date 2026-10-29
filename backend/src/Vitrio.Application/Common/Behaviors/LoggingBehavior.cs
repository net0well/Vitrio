using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Common.Behaviors;

public sealed class LoggingBehavior<TMessage, TResponse>(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;
        using var activity = ApplicationTelemetry.ActivitySource.StartActivity(messageName);
        var startedAt = Stopwatch.GetTimestamp();

        var response = await next(message, cancellationToken);

        var elapsedMilliseconds = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
        if (response is Result { IsFailure: true } failure)
        {
            activity?.SetTag("result.error_code", failure.Error.Code);
            logger.LogInformation(
                "{MessageName} failed with {ErrorCode} in {ElapsedMilliseconds:0.0} ms",
                messageName,
                failure.Error.Code,
                elapsedMilliseconds);
        }
        else
        {
            logger.LogInformation("{MessageName} handled in {ElapsedMilliseconds:0.0} ms", messageName, elapsedMilliseconds);
        }

        return response;
    }
}
