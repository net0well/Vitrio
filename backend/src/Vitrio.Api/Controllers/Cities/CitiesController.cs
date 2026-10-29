using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.GetCity;
using Vitrio.Application.Locations.LocateCity;
using Vitrio.Application.Locations.SearchCities;

namespace Vitrio.Api.Controllers.Cities;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/cities")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Search)]
public sealed class CitiesController(ISender sender) : ApiControllerBase
{
    private const int OneDay = 86_400;

    [HttpGet("locate")]
    [EndpointSummary("Descobre a cidade de um ponto")]
    [ProducesResponseType<LocateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Locate([FromQuery] double? lat, [FromQuery] double? lng, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new LocateCityQuery(lat, lng), cancellationToken));

    [HttpGet]
    [ResponseCache(Duration = OneDay, Location = ResponseCacheLocation.Any)]
    [EndpointSummary("Busca cidades pelo nome")]
    [ProducesResponseType<IReadOnlyList<CitySummary>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string? state,
        [FromQuery] int? limit,
        CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new SearchCitiesQuery(search ?? string.Empty, state, limit), cancellationToken));

    [HttpGet("{ibgeCode:int}")]
    [ResponseCache(Duration = OneDay, Location = ResponseCacheLocation.Any)]
    [EndpointSummary("Mostra uma cidade pelo código IBGE")]
    [ProducesResponseType<CityDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int ibgeCode, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetCityQuery(ibgeCode), cancellationToken));
}
