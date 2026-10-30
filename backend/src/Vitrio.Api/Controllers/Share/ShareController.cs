using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Api.Share;
using Vitrio.Application.Providers.GetAssetLinks;
using Vitrio.Application.Providers.GetSharePage;

namespace Vitrio.Api.Controllers.Share;

[ApiVersionNeutral]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Profile)]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ShareController(ISender sender) : ApiControllerBase
{
    private const string HtmlContentType = "text/html; charset=utf-8";
    private const string FiveMinutes = "public, max-age=300";

    [HttpGet("/p/{id:guid}")]
    public async Task<IActionResult> Profile(Guid id, CancellationToken cancellationToken)
    {
        var page = (await sender.Send(new GetSharePageQuery(id), cancellationToken)).Value;
        Response.Headers.CacheControl = FiveMinutes;
        Response.Headers.ContentSecurityPolicy = SharePage.ContentSecurityPolicy(page);
        return new ContentResult
        {
            Content = SharePage.Render(page),
            ContentType = HtmlContentType,
            StatusCode = page.Found ? StatusCodes.Status200OK : StatusCodes.Status404NotFound
        };
    }

    [HttpGet("/.well-known/assetlinks.json")]
    public async Task<IActionResult> AssetLinks(CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = FiveMinutes;
        return ToActionResult(await sender.Send(new GetAssetLinksQuery(), cancellationToken));
    }
}
