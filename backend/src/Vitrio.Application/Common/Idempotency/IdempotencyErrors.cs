using Vitrio.Shared.Results;

namespace Vitrio.Application.Common.Idempotency;

public static class IdempotencyErrors
{
    public static readonly Error KeyReused = Error.Unprocessable(
        "idempotency.key_reused",
        "Esta chave de envio já foi usada com outros dados. Gere uma chave nova para enviar de novo.");
}
