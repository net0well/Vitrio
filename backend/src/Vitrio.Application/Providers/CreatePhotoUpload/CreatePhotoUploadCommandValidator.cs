using FluentValidation;
using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Providers.CreatePhotoUpload;

internal sealed class CreatePhotoUploadCommandValidator : AbstractValidator<CreatePhotoUploadCommand>
{
    public CreatePhotoUploadCommandValidator()
    {
        RuleFor(command => command.Purpose).IsInEnum();
        RuleFor(command => command.ContentType)
            .Must(PhotoKeys.AcceptedContentTypes.Contains).WithMessage("Envie uma foto JPEG, PNG ou WebP.");
        RuleFor(command => command.ContentLength)
            .InclusiveBetween(1, PhotoKeys.MaxUploadBytes).WithMessage("A foto pode ter até 10 MB.");
    }
}
