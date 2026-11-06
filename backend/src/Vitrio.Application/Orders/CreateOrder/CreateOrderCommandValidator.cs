using FluentValidation;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.CreateOrder;

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.IdempotencyKey).ValidIdempotencyKey();
        RuleFor(command => command.ProviderId).NotEmpty().WithMessage("Escolha o profissional.");
        RuleFor(command => command.AddressId).NotEmpty().WithMessage("Escolha o endereço do serviço.");
        RuleFor(command => command.PreferredDay).NotEmpty().WithMessage("Escolha o dia.");
        RuleFor(command => command.PreferredPeriod).NotNull().IsInEnum().WithMessage("Escolha o período.");
        RuleFor(command => command.PhotoKeys)
            .Must(keys => keys.Count <= Order.MaxPhotos).WithMessage("Envie até 5 fotos.")
            .Must(keys => keys.Distinct(StringComparer.Ordinal).Count() == keys.Count).WithMessage("Cada foto entra uma vez só.");
    }
}
