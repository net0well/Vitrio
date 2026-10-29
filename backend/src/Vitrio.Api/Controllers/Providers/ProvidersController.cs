using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.GetPublicProvider;

namespace Vitrio.Api.Controllers.Providers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/providers")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Profile)]
public sealed class ProvidersController(ISender sender) : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    [EndpointSummary("Mostra o perfil público de uma vitrine publicada")]
    [ProducesResponseType<ProviderPublicResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, [FromQuery] double? lat, [FromQuery] double? lng, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetPublicProviderQuery(id, lat, lng), cancellationToken));
}
