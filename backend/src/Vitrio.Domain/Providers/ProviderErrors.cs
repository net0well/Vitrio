using Vitrio.Shared.Results;

namespace Vitrio.Domain.Providers;

public static class ProviderErrors
{
    public static readonly Error AlreadyExists = Error.Conflict("provider.already_exists", "Você já tem uma vitrine.");

    public static readonly Error NotFound = Error.NotFound("provider.not_found", "Vitrine não encontrada.");

    public static readonly Error InvalidHeadline =
        Error.Validation("provider.invalid_headline", "Informe a profissão com até 60 caracteres.");

    public static readonly Error InvalidAbout =
        Error.Validation("provider.invalid_about", "Escreva sobre você e o seu trabalho com 40 a 600 caracteres.");

    public static readonly Error ContactInText =
        Error.Validation("provider.contact_in_text", "Use o botão de WhatsApp para contato. Tire telefone, e-mail e links do texto.");

    public static readonly Error InvalidYearsOfExperience =
        Error.Validation("provider.invalid_years_of_experience", "Informe de 0 a 60 anos de experiência.");

    public static readonly Error InvalidCategories =
        Error.Validation("provider.invalid_categories", "Escolha de 1 a 3 categorias e marque uma como principal.");

    public static readonly Error UnknownCategory = Error.Validation("provider.unknown_category", "Categoria não encontrada.");

    public static readonly Error InvalidRadius = Error.Validation("provider.invalid_radius", "O raio de atendimento vai de 1 a 50 km.");

    public static readonly Error InvalidNeighborhoods =
        Error.Validation("provider.invalid_neighborhoods", "Informe até 30 bairros, cada um com 2 a 60 caracteres.");

    public static readonly Error InvalidServices =
        Error.Validation("provider.invalid_services", "Cadastre até 20 serviços, cada um com nome de até 60 caracteres.");

    public static readonly Error InvalidPrice =
        Error.Validation("provider.invalid_price", "Informe o preço de R$ 1,00 a R$ 100.000,00 ou marque \"Sob orçamento\".");

    public static readonly Error InvalidAvailability = Error.Validation(
        "provider.invalid_availability",
        "Use faixas de 30 em 30 minutos, até 3 por dia e sem sobreposição.");

    public static readonly Error NotPublished = Error.Conflict("provider.not_published", "Publique a vitrine primeiro.");

    public static readonly Error Suspended = Error.Conflict("provider.suspended", "Esta vitrine está suspensa.");

    public static MissingFieldsError Incomplete(IReadOnlyList<string> missing) =>
        new("provider.incomplete", "Complete a vitrine antes de publicar.", missing);
}
