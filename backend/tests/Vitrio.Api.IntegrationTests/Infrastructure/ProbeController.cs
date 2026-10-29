using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.Controllers;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Abstractions;
using Vitrio.Shared.Results;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/probe")]
[EnableRateLimiting(RateLimitPolicies.Search)]
public sealed class ProbeController : ApiControllerBase
{
    public const string FailureCode = "probe.failed";
    public const string FailureMessage = "A sondagem falhou.";
    public const string CrashMessage = "connection string Host=db;Password=segredo";

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

    [HttpGet("public-reference")]
    [AllowAnonymous]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult PublicReference() => ToActionResult(Result<ProbeResponse>.Success(new ProbeResponse("reference")));

    [HttpPost("echo")]
    [AllowAnonymous]
    public IActionResult Echo(ProbeRequest request) => Ok(new ProbeResponse(request.Name));

    [HttpGet("crash")]
    [AllowAnonymous]
    public IActionResult Crash() => throw new InvalidOperationException(CrashMessage);

    [HttpGet("concurrency-conflict")]
    [AllowAnonymous]
    public IActionResult ConcurrencyConflict() => throw new ConcurrencyConflictException();

    [HttpGet("private")]
    [Authorize]
    public IActionResult Private() => NoContent();

    [HttpGet("limited")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiFactory.TightRateLimitPolicy)]
    public IActionResult Limited() => NoContent();
}
