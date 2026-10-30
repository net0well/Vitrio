using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Search.Common;
using Vitrio.Application.Search.GetHome;

namespace Vitrio.Api.Controllers.Home;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/home")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Search)]
public sealed class HomeController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("Mostra a Início: categorias, bem avaliados e se há prestadores no ponto do GPS ou na cidade escolhida")]
    [ProducesResponseType<HomeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Get(
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] int? cityIbgeCode,
        CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetHomeQuery(lat, lng, cityIbgeCode), cancellationToken));
}
