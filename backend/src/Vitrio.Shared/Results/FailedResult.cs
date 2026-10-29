using System.Reflection;

namespace Vitrio.Shared.Results;

public static class FailedResult<TResult>
{
    private static readonly Func<Error, TResult> Factory = CreateFactory();

    public static TResult From(Error error) => Factory(error);

    private static Func<Error, TResult> CreateFactory()
    {
        var resultType = typeof(TResult);

        if (resultType == typeof(Result))
        {
            return error => (TResult)(object)Result.Failure(error);
        }

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failure = resultType.GetMethod(
                nameof(Result.Failure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                [typeof(Error)])!;

            return failure.CreateDelegate<Func<Error, TResult>>();
        }

        return _ => throw new InvalidOperationException(
            $"Messages that can fail must return {nameof(Result)} or {nameof(Result)}<T>, but '{resultType.Name}' was found.");
    }
}
