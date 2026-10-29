namespace Vitrio.Shared.Results;

public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors)
    : Error("validation", "Um ou mais campos são inválidos.", ErrorType.Validation);
