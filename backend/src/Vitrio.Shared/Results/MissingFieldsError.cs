namespace Vitrio.Shared.Results;

public sealed record MissingFieldsError(string Code, string Message, IReadOnlyList<string> Missing)
    : Error(Code, Message, ErrorType.Unprocessable);
