namespace Vitrio.Domain.Locations;

public static class BrazilStates
{
    public static readonly IReadOnlyList<string> All =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    private static readonly HashSet<string> Known = new(All, StringComparer.Ordinal);

    public static bool IsState(string? state) => state is not null && Known.Contains(state);
}
