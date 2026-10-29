using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Accounts.ListSessions;
using Vitrio.Application.Accounts.RevokeSession;

namespace Vitrio.Api.Controllers.Me;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/me")]
[Authorize]
public sealed class MeController(ISender sender) : ApiControllerBase
{
    [HttpPut("password")]
    [EndpointSummary("Troca a senha e desconecta os outros aparelhos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpGet("sessions")]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Lista os aparelhos conectados")]
    [ProducesResponseType<IReadOnlyList<SessionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListSessions(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ListSessionsQuery(), cancellationToken));

    [HttpDelete("sessions/{id:guid}")]
    [EndpointSummary("Encerra a sessão de um aparelho")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new RevokeSessionCommand(id), cancellationToken));
}
