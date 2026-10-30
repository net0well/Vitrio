using FluentValidation;
using Vitrio.Application.Search.Common;

namespace Vitrio.Application.Search.GetHome;

internal sealed class GetHomeQueryValidator : AbstractValidator<GetHomeQuery>
{
    public GetHomeQueryValidator() => Include(new SearchLocationValidator());
}
