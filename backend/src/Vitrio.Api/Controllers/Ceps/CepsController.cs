using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Locations.LookupCep;

namespace Vitrio.Api.Controllers.Ceps;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/ceps")]
[Authorize]
[EnableRateLimiting(RateLimitPolicies.Search)]
public sealed class CepsController(ISender sender) : ApiControllerBase
{
    [HttpGet("{cep}")]
    [EndpointSummary("Consulta um CEP para preencher o endereço")]
    [ProducesResponseType<CepResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Lookup(string cep, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new LookupCepQuery(cep), cancellationToken));
}
