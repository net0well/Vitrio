using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.DeleteAccount;
using Vitrio.Application.Accounts.DeleteAddress;
using Vitrio.Application.Accounts.GetMe;
using Vitrio.Application.Accounts.ListAddresses;
using Vitrio.Application.Accounts.ListSessions;
using Vitrio.Application.Accounts.RemoveDevice;
using Vitrio.Application.Accounts.RevokeSession;

namespace Vitrio.Api.Controllers.Me;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/me")]
[Authorize]
public sealed class MeController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Mostra os dados da conta")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetMeQuery(), cancellationToken));

    [HttpPatch]
    [EndpointSummary("Altera o nome de exibição ou o celular")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe(UpdateMeRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpDelete]
    [EndpointSummary("Exclui a conta e encerra todas as sessões")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new DeleteAccountCommand(), cancellationToken));

    [HttpGet("addresses")]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Lista os endereços salvos")]
    [ProducesResponseType<IReadOnlyList<AddressResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListAddresses(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ListAddressesQuery(), cancellationToken));

    [HttpPost("addresses")]
    [EndpointSummary("Salva um endereço a partir do CEP")]
    [ProducesResponseType<AddressResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AddAddress(AddressRequest request, CancellationToken cancellationToken) =>
        ToCreatedResult(await sender.Send(request.ToAddCommand(), cancellationToken));

    [HttpPut("addresses/{id:guid}")]
    [EndpointSummary("Altera um endereço salvo")]
    [ProducesResponseType<AddressResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateAddress(Guid id, AddressRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToUpdateCommand(id), cancellationToken));

    [HttpDelete("addresses/{id:guid}")]
    [EndpointSummary("Apaga um endereço salvo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new DeleteAddressCommand(id), cancellationToken));

    [HttpPut("devices/{installationId}")]
    [EndpointSummary("Registra o aparelho para receber notificações")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterDevice(string installationId, RegisterDeviceRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(installationId), cancellationToken));

    [HttpDelete("devices/{installationId}")]
    [EndpointSummary("Para de enviar notificações para o aparelho")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveDevice(string installationId, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new RemoveDeviceCommand(installationId), cancellationToken));

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
