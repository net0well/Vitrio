using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Api.Controllers.Auth;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public sealed class AuthController(ISender sender) : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken) =>
        ToAcceptedResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("verify-email")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("resend-code")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendCode(ResendCodeRequest request, CancellationToken cancellationToken) =>
        ToAcceptedResult(await sender.Send(request.ToCommand(), cancellationToken));
}
