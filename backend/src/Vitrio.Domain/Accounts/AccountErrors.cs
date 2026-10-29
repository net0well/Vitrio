using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public static class AccountErrors
{
    public static readonly Error InvalidEmail = Error.Validation("account.invalid_email", "Informe um e-mail válido.");

    public static readonly Error InvalidDisplayName =
        Error.Validation("account.invalid_display_name", "O nome precisa ter de 2 a 60 caracteres.");

    public static readonly Error DisplayNameWithContact =
        Error.Validation("account.display_name_with_contact", "O nome não pode ter telefone, e-mail ou link.");

    public static readonly Error InvalidCode =
        Error.Validation("auth.invalid_code", "Código inválido ou vencido. Peça um novo código.");

    public static readonly Error NotPending =
        Error.Conflict("account.not_pending", "Esta conta já foi confirmada.");

    public static readonly Error EmailUnavailable = new(
        "email.unavailable",
        "Não conseguimos enviar o e-mail agora. Tente de novo em alguns minutos.",
        ErrorType.Unavailable)
    {
        RetryAfter = TimeSpan.FromSeconds(60)
    };

    public static Error TooManyCodes(TimeSpan retryAfter) => new(
        "auth.too_many_codes",
        "Você pediu muitos códigos. Espere um pouco e tente de novo.",
        ErrorType.RateLimited)
    {
        RetryAfter = retryAfter
    };
}
