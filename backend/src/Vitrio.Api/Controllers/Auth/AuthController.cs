using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Logout;

namespace Vitrio.Api.Controllers.Auth;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(ISender sender) : ApiControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [EndpointSummary("Cria a conta e envia o código de confirmação por e-mail")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken) =>
        ToAcceptedResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [EndpointSummary("Confirma o e-mail com o código e abre a sessão")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("resend-code")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [EndpointSummary("Envia um novo código por e-mail")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ResendCode(ResendCodeRequest request, CancellationToken cancellationToken) =>
        ToAcceptedResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [EndpointSummary("Entra com e-mail e senha")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [EndpointSummary("Troca o refresh token por um novo par de tokens")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("password/forgot")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [EndpointSummary("Envia o código para redefinir a senha")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken) =>
        ToAcceptedResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("password/reset")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [EndpointSummary("Redefine a senha com o código e abre uma sessão nova")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("logout")]
    [Authorize]
    [EndpointSummary("Encerra a sessão deste aparelho ou de todos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new LogoutCommand(request.AllDevices), cancellationToken));
}
