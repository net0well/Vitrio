using FluentValidation;
using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Common.Behaviors;

public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        var errors = failures
            .GroupBy(failure => failure.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        return FailedResult<TResponse>.From(new ValidationError(errors));
    }
}
