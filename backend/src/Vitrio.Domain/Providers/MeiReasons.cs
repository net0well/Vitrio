namespace Vitrio.Domain.Providers;

public static class MeiReasons
{
    public const string NotMei = "mei.not_mei";
    public const string Inactive = "mei.inactive";
    public const string NotFound = "mei.cnpj_not_found";
    public const string LookupUnavailable = "mei.lookup_unavailable";

    public static string For(MeiCheckOutcome outcome) => outcome switch
    {
        MeiCheckOutcome.NotMei => NotMei,
        MeiCheckOutcome.Inactive => Inactive,
        MeiCheckOutcome.NotFound => NotFound,
        _ => LookupUnavailable
    };

    public static string? MessageFor(string? code) => code switch
    {
        NotMei => "CNPJ não é optante pelo MEI.",
        Inactive => "CNPJ não está ativo na Receita Federal.",
        NotFound => "CNPJ não encontrado na Receita Federal.",
        LookupUnavailable => "Não conseguimos consultar o CNPJ agora.",
        _ => null
    };
}
