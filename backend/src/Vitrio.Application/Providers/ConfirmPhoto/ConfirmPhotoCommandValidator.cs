using FluentValidation;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.ConfirmPhoto;

internal sealed class ConfirmPhotoCommandValidator : AbstractValidator<ConfirmPhotoCommand>
{
    public ConfirmPhotoCommandValidator()
    {
        RuleFor(command => command.ObjectKey).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Caption).MaximumLength(PortfolioPhoto.CaptionMaxLength);
    }
}
