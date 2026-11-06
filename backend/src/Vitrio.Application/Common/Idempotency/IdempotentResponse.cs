using System.Reflection;
using System.Text.Json;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Common.Idempotency;

public static class IdempotentResponse<TResponse>
{
    private static readonly JsonSerializerOptions Options = JsonSerializerOptions.Web;
    private static readonly Type? ValueType = FindValueType();
    private static readonly PropertyInfo? ValueProperty = ValueType is null ? null : typeof(TResponse).GetProperty(nameof(Result<object>.Value));
    private static readonly Func<object?, TResponse> Success = CreateSuccess();

    public static string Serialize(TResponse response) =>
        ValueType is null ? "null" : JsonSerializer.Serialize(ValueProperty!.GetValue(response), ValueType, Options);

    public static TResponse Restore(string body) =>
        Success(ValueType is null ? null : JsonSerializer.Deserialize(body, ValueType, Options));

    private static Type? FindValueType()
    {
        var responseType = typeof(TResponse);
        return responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>)
            ? responseType.GetGenericArguments()[0]
            : null;
    }

    private static Func<object?, TResponse> CreateSuccess()
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return _ => (TResponse)(object)Result.Success();
        }

        if (ValueType is not null)
        {
            var success = typeof(TResponse).GetMethod(
                nameof(Result.Success),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                [ValueType])!;
            return value => (TResponse)success.Invoke(null, [value])!;
        }

        return _ => throw new InvalidOperationException(
            $"Idempotent commands must return {nameof(Result)} or {nameof(Result)}<T>, but '{typeof(TResponse).Name}' was found.");
    }
}
