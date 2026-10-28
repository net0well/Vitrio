using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.Controllers;
using Vitrio.Api.RateLimiting;
using Vitrio.Domain.Common;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/probe")]
[EnableRateLimiting(RateLimitPolicies.Search)]
public sealed class ProbeController : ApiControllerBase
{
    public const string FailureCode = "probe.failed";
    public const string FailureMessage = "A sondagem falhou.";

    [HttpGet("errors/{type}")]
    [AllowAnonymous]
    public IActionResult Fail(ErrorType type) => ToActionResult(Result.Failure(new Error(FailureCode, FailureMessage, type)));

    [HttpGet("validation")]
    [AllowAnonymous]
    public IActionResult Invalid() => ToActionResult(Result.Failure(new ValidationError(new Dictionary<string, string[]>
    {
        ["DisplayName"] = ["Informe o nome."],
        ["Address.PostalCode"] = ["CEP inválido."]
    })));

    [HttpGet("value")]
    [AllowAnonymous]
    public IActionResult Value() => ToActionResult(Result<ProbeResponse>.Success(new ProbeResponse("ok")));

    [HttpGet("nothing")]
    [AllowAnonymous]
    public IActionResult Nothing() => ToActionResult(Result.Success());

    [HttpPost("echo")]
    [AllowAnonymous]
    public IActionResult Echo(ProbeRequest request) => Ok(new ProbeResponse(request.Name));

    [HttpGet("private")]
    [Authorize]
    public IActionResult Private() => NoContent();

    [HttpGet("limited")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiFactory.TightRateLimitPolicy)]
    public IActionResult Limited() => NoContent();
}

public sealed record ProbeRequest(string Name);

public sealed record ProbeResponse(string Name);
