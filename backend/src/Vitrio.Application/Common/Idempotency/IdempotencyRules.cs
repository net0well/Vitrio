using FluentValidation;

namespace Vitrio.Application.Common.Idempotency;

public static class IdempotencyRules
{
    public static IRuleBuilderOptions<T, Guid> ValidIdempotencyKey<T>(this IRuleBuilder<T, Guid> rule) =>
        rule.NotEmpty().WithMessage("Envie o cabeçalho Idempotency-Key com um identificador novo para cada envio.");
}
