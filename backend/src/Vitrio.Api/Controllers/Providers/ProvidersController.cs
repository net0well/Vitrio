using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.GetPublicProvider;
using Vitrio.Application.Providers.ListPublicPhotos;
using Vitrio.Application.Search.Common;

namespace Vitrio.Api.Controllers.Providers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/providers")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Profile)]
public sealed class ProvidersController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.Search)]
    [EndpointSummary("Busca prestadores que atendem o ponto do GPS ou a cidade escolhida, com texto, filtros e ordenação")]
    [ProducesResponseType<ProviderSearchPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Search([FromQuery] SearchProvidersRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    [EndpointSummary("Mostra o perfil público de uma vitrine publicada")]
    [ProducesResponseType<ProviderPublicResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, [FromQuery] double? lat, [FromQuery] double? lng, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetPublicProviderQuery(id, lat, lng), cancellationToken));

    [HttpGet("{id:guid}/photos")]
    [EndpointSummary("Lista as fotos de trabalhos de uma vitrine publicada, 12 por página")]
    [ProducesResponseType<PublicPhotosPageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Photos(Guid id, [FromQuery] int? page, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ListPublicPhotosQuery(id, page ?? 1), cancellationToken));
}
