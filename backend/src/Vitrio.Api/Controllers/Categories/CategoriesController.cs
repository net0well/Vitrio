using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.ListCategories;

namespace Vitrio.Api.Controllers.Categories;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/categories")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Search)]
public sealed class CategoriesController(ISender sender) : ApiControllerBase
{
    private const int OneHour = 3_600;

    [HttpGet]
    [ResponseCache(Duration = OneHour, Location = ResponseCacheLocation.Any)]
    [EndpointSummary("Lista as categorias de serviço")]
    [ProducesResponseType<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ListCategoriesQuery(), cancellationToken));
}
