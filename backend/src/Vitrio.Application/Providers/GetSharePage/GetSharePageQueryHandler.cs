using System.Globalization;
using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Services;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.GetSharePage;

public sealed class GetSharePageQueryHandler(IPublicProfileService profiles, IPublicLinks links)
    : IQueryHandler<GetSharePageQuery, Result<SharePageResponse>>
{
    private const int MinReviewsForRating = 3;

    private static readonly CultureInfo Portuguese = CultureInfo.GetCultureInfo("pt-BR");

    public async ValueTask<Result<SharePageResponse>> Handle(GetSharePageQuery query, CancellationToken cancellationToken)
    {
        var snapshot = await profiles.GetSnapshotAsync(query.ProviderId, cancellationToken);
        if (snapshot is null)
        {
            return new SharePageResponse(false, links.PlayStore, null, null, null, null, null, null);
        }

        var rating = snapshot.IsRated(MinReviewsForRating)
            ? string.Create(Portuguese, $"Nota {snapshot.RatingScore:0.0} ({snapshot.ReviewCount} avaliações)")
            : "Novo no Vitrio";
        return new SharePageResponse(
            true,
            links.PlayStore,
            links.ProviderProfile(snapshot.Id),
            snapshot.DisplayName,
            snapshot.Headline,
            $"{snapshot.City.Name}, {snapshot.City.State}",
            rating,
            snapshot.AvatarUrl ?? snapshot.FirstPhotos.FirstOrDefault()?.Url);
    }
}
