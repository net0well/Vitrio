using FluentValidation;

namespace Vitrio.Application.Accounts.RefreshSession;

internal sealed class RefreshSessionCommandValidator : AbstractValidator<RefreshSessionCommand>
{
    public const int MaxTokenLength = 100;

    public RefreshSessionCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(MaxTokenLength);
    }
}
