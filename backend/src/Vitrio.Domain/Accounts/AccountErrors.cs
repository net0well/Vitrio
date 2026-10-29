using Vitrio.Shared.Results;

namespace Vitrio.Domain.Accounts;

public static class AccountErrors
{
    public static readonly Error InvalidEmail = Error.Validation("account.invalid_email", "Informe um e-mail válido.");

    public static readonly Error InvalidDisplayName =
        Error.Validation("account.invalid_display_name", "O nome precisa ter de 2 a 60 caracteres.");

    public static readonly Error DisplayNameWithContact =
        Error.Validation("account.display_name_with_contact", "O nome não pode ter telefone, e-mail ou link.");

    public static readonly Error InvalidPhoneNumber =
        Error.Validation("account.invalid_phone_number", "Informe um celular com DDD, como (16) 99123-4567.");

    public static readonly Error InvalidCep = Error.Validation("address.invalid_cep", "Informe um CEP com 8 números.");

    public static readonly Error CepNotFound =
        Error.Validation("address.cep_not_found", "Não encontramos este CEP. Preencha a rua, o bairro e a cidade.");

    public static readonly Error CepUnavailable = new(
        "address.cep_unavailable",
        "Não conseguimos consultar o CEP agora. Preencha a rua, o bairro e a cidade ou tente de novo.",
        ErrorType.Unavailable)
    {
        RetryAfter = TimeSpan.FromSeconds(30)
    };

    public static readonly Error StreetRequired = Error.Validation("address.street_required", "Informe a rua.");

    public static readonly Error NeighborhoodRequired = Error.Validation("address.neighborhood_required", "Informe o bairro.");

    public static readonly Error CityNotFound = Error.Validation("address.city_not_found", "Escolha a cidade na lista.");

    public static readonly Error InvalidAddressLabel =
        Error.Validation("address.invalid_label", "Dê um nome de até 40 caracteres ao endereço, como Casa ou Trabalho.");

    public static readonly Error InvalidAddressNumber = Error.Validation("address.invalid_number", "Informe o número, com até 10 caracteres.");

    public static readonly Error InvalidAddressComplement =
        Error.Validation("address.invalid_complement", "O complemento pode ter até 60 caracteres, sem telefone ou link.");

    public static readonly Error AccountNotFound = Error.NotFound("account.not_found", "Conta não encontrada.");

    public static readonly Error AddressNotFound = Error.NotFound("address.not_found", "Endereço não encontrado.");

    public static readonly Error TooManyAddresses =
        Error.Conflict("address.limit_reached", "Você já tem 10 endereços. Apague um para salvar outro.");

    public static readonly Error DeviceNotFound = Error.NotFound("device.not_found", "Aparelho não encontrado.");

    public static readonly Error InvalidCode =
        Error.Validation("auth.invalid_code", "Código inválido ou vencido. Peça um novo código.");

    public static readonly Error NotPending =
        Error.Conflict("account.not_pending", "Esta conta já foi confirmada.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("auth.invalid_credentials", "E-mail ou senha incorretos.");

    public static readonly Error WrongCurrentPassword =
        Error.Forbidden("auth.invalid_credentials", "A senha atual está incorreta.");

    public static readonly Error EmailNotVerified =
        Error.Conflict("account.email_not_verified", "Confirme seu e-mail com o código que enviamos para entrar.");

    public static readonly Error Suspended =
        Error.Forbidden("account.suspended", "Sua conta está suspensa. Fale com o suporte do Vitrio.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("auth.invalid_refresh_token", "Sua sessão terminou. Entre de novo.");

    public static readonly Error SessionNotFound =
        Error.NotFound("session.not_found", "Sessão não encontrada.");

    public static readonly Error EmailUnavailable = new(
        "email.unavailable",
        "Não conseguimos enviar o e-mail agora. Tente de novo em alguns minutos.",
        ErrorType.Unavailable)
    {
        RetryAfter = TimeSpan.FromSeconds(60)
    };

    public static Error LockedOut(TimeSpan retryAfter) => new(
        "auth.locked_out",
        "Muitas tentativas com a senha errada. Espere alguns minutos ou redefina a senha.",
        ErrorType.RateLimited)
    {
        RetryAfter = retryAfter
    };

    public static Error TooManyCodes(TimeSpan retryAfter) => new(
        "auth.too_many_codes",
        "Você pediu muitos códigos. Espere um pouco e tente de novo.",
        ErrorType.RateLimited)
    {
        RetryAfter = retryAfter
    };
}
