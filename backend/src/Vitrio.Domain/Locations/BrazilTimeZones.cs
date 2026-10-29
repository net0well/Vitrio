using Vitrio.Domain.Common;

namespace Vitrio.Domain.Locations;

public static class BrazilTimeZones
{
    public const string FernandoDeNoronha = "fernando de noronha";

    private static readonly Dictionary<string, string> ByState = new(StringComparer.Ordinal)
    {
        ["AC"] = "America/Rio_Branco",
        ["AL"] = "America/Maceio",
        ["AP"] = "America/Belem",
        ["AM"] = "America/Manaus",
        ["BA"] = "America/Bahia",
        ["CE"] = "America/Fortaleza",
        ["DF"] = "America/Sao_Paulo",
        ["ES"] = "America/Sao_Paulo",
        ["GO"] = "America/Sao_Paulo",
        ["MA"] = "America/Fortaleza",
        ["MT"] = "America/Cuiaba",
        ["MS"] = "America/Campo_Grande",
        ["MG"] = "America/Sao_Paulo",
        ["PA"] = "America/Belem",
        ["PB"] = "America/Fortaleza",
        ["PR"] = "America/Sao_Paulo",
        ["PE"] = "America/Recife",
        ["PI"] = "America/Fortaleza",
        ["RJ"] = "America/Sao_Paulo",
        ["RN"] = "America/Fortaleza",
        ["RS"] = "America/Sao_Paulo",
        ["RO"] = "America/Porto_Velho",
        ["RR"] = "America/Boa_Vista",
        ["SC"] = "America/Sao_Paulo",
        ["SP"] = "America/Sao_Paulo",
        ["SE"] = "America/Maceio",
        ["TO"] = "America/Araguaina"
    };

    // Lei 12.876/2013 put these Amazonas municipalities back on Acre time (UTC-5).
    private static readonly HashSet<string> WesternAmazonas = new(StringComparer.Ordinal)
    {
        "atalaia do norte", "benjamin constant", "boca do acre", "eirunepe", "envira", "guajara", "ipixuna",
        "itamarati", "jutai", "labrea", "pauini", "sao paulo de olivenca", "tabatinga"
    };

    public static string For(string state, string cityName)
    {
        var name = SearchText.Normalize(cityName);
        if (state == "PE" && name == FernandoDeNoronha)
        {
            return "America/Noronha";
        }

        if (state == "AM" && WesternAmazonas.Contains(name))
        {
            return "America/Eirunepe";
        }

        return ByState.TryGetValue(state, out var zone)
            ? zone
            : throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown Brazilian state.");
    }
}
