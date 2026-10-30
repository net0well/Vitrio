using FluentValidation;

namespace Vitrio.Application.Providers.ListPublicPhotos;

internal sealed class ListPublicPhotosQueryValidator : AbstractValidator<ListPublicPhotosQuery>
{
    public ListPublicPhotosQueryValidator()
    {
        RuleFor(query => query.Page).InclusiveBetween(1, ListPublicPhotosQuery.MaxPage);
    }
}
