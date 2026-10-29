using FluentValidation;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Locations.LookupCep;

internal sealed class LookupCepQueryValidator : AbstractValidator<LookupCepQuery>
{
    public LookupCepQueryValidator()
    {
        RuleFor(query => query.Cep).Must(cep => Cep.Create(cep).IsSuccess).WithMessage(AccountErrors.InvalidCep.Message);
    }
}
