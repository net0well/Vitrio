using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.Authorization;
using Vitrio.Api.Controllers.Idempotency;
using Vitrio.Api.Controllers.Orders;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.CompleteProviderOrder;
using Vitrio.Application.Orders.GetProviderAgenda;
using Vitrio.Application.Orders.ListProviderOrders;
using Vitrio.Application.Orders.ViewProviderOrder;

namespace Vitrio.Api.Controllers.Provider;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/provider")]
[Authorize(Policy = Policies.Provider)]
public sealed class ProviderOrdersController(ISender sender) : ApiControllerBase
{
    [HttpGet("orders")]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Lista os pedidos recebidos por aba: novos, em andamento e finalizados")]
    [ProducesResponseType<OrderProviderPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List([FromQuery] ProviderOrderTab? tab, [FromQuery] int? page, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ListProviderOrdersQuery(tab ?? ProviderOrderTab.New, page ?? 1), cancellationToken));

    [HttpGet("orders/{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Mostra o pedido; endereço e contato do cliente só enquanto liberados")]
    [ProducesResponseType<OrderProviderView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ViewProviderOrderCommand(id), cancellationToken));

    [HttpPost("orders/{id:guid}/quote")]
    [EnableRateLimiting(RateLimitPolicies.Quotes)]
    [EndpointSummary("Envia ou substitui o orçamento do pedido")]
    [ProducesResponseType<OrderProviderView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendQuote(
        Guid id,
        [FromHeader(Name = IdempotencyRequest.Header)] Guid? idempotencyKey,
        SendQuoteRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(id, idempotencyKey, IdempotencyRequest.Hash(request)), cancellationToken));

    [HttpPost("orders/{id:guid}/decline")]
    [EndpointSummary("Recusa o pedido com um motivo")]
    [ProducesResponseType<OrderProviderView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Decline(Guid id, DeclineOrderRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(id), cancellationToken));

    [HttpPost("orders/{id:guid}/reschedule")]
    [EndpointSummary("Ajusta o horário de um serviço agendado")]
    [ProducesResponseType<OrderProviderView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reschedule(Guid id, RescheduleOrderRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(id), cancellationToken));

    [HttpPost("orders/{id:guid}/cancel")]
    [EndpointSummary("Cancela o serviço agendado com um motivo")]
    [ProducesResponseType<OrderProviderView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancelOrderRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToProviderCommand(id), cancellationToken));

    [HttpPost("orders/{id:guid}/complete")]
    [EndpointSummary("Marca o serviço como concluído")]
    [ProducesResponseType<OrderProviderView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new CompleteProviderOrderCommand(id), cancellationToken));

    [HttpGet("agenda")]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Lista os serviços agendados, os orçamentos que aguardam o cliente e os feitos num intervalo de até 42 dias")]
    [ProducesResponseType<IReadOnlyList<AgendaItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Agenda([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetProviderAgendaQuery(from, to), cancellationToken));
}
