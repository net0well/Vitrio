using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Favorites.AddFavorite;
using Vitrio.Application.Favorites.Common;
using Vitrio.Application.Favorites.ListFavorites;
using Vitrio.Application.Favorites.RemoveFavorite;

namespace Vitrio.Api.Controllers.Favorites;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/me/favorites")]
[Authorize]
public sealed class FavoritesController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Lista os prestadores salvos nos favoritos, do mais recente para o mais antigo")]
    [ProducesResponseType<FavoritesPageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] int? page, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ListFavoritesQuery(page ?? 1), cancellationToken));

    [HttpPut("{providerId:guid}")]
    [EndpointSummary("Salva um prestador nos favoritos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(Guid providerId, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new AddFavoriteCommand(providerId), cancellationToken));

    [HttpDelete("{providerId:guid}")]
    [EndpointSummary("Tira um prestador dos favoritos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Remove(Guid providerId, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new RemoveFavoriteCommand(providerId), cancellationToken));
}
