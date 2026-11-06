using FluentValidation;
using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Orders.CreateOrderPhotoUpload;

internal sealed class CreateOrderPhotoUploadCommandValidator : AbstractValidator<CreateOrderPhotoUploadCommand>
{
    public CreateOrderPhotoUploadCommandValidator()
    {
        RuleFor(command => command.ContentType)
            .Must(PhotoKeys.AcceptedContentTypes.Contains).WithMessage("Envie uma foto JPEG, PNG ou WebP.");
        RuleFor(command => command.ContentLength)
            .InclusiveBetween(1, PhotoKeys.MaxUploadBytes).WithMessage("A foto pode ter até 10 MB.");
    }
}
