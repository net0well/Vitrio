using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.Controllers.Idempotency;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.CompleteCustomerOrder;
using Vitrio.Application.Orders.CopyOrderPhotos;
using Vitrio.Application.Orders.DeclineQuote;
using Vitrio.Application.Orders.GetCustomerOrder;
using Vitrio.Application.Orders.GetOrderSuggestions;
using Vitrio.Application.Orders.ListCustomerOrders;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Search.Common;

namespace Vitrio.Api.Controllers.Orders;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/orders")]
[Authorize]
public sealed class OrdersController(ISender sender) : ApiControllerBase
{
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Orders)]
    [EndpointSummary("Envia um pedido de orçamento para um profissional")]
    [ProducesResponseType<OrderCustomerView>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = IdempotencyRequest.Header)] Guid? idempotencyKey,
        CreateOrderRequest request,
        CancellationToken cancellationToken) =>
        ToCreatedResult(await sender.Send(request.ToCommand(idempotencyKey, IdempotencyRequest.Hash(request)), cancellationToken));

    [HttpPost("photos/upload-url")]
    [EnableRateLimiting(RateLimitPolicies.Uploads)]
    [EndpointSummary("Gera o endereço para enviar uma foto do pedido direto ao storage")]
    [ProducesResponseType<PhotoUploadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreatePhotoUpload(OrderPhotoUploadRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("{id:guid}/photos/copies")]
    [EnableRateLimiting(RateLimitPolicies.Uploads)]
    [EndpointSummary("Copia as fotos do pedido para usar num pedido novo")]
    [ProducesResponseType<IReadOnlyList<OrderPhotoCopyResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CopyPhotos(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new CopyOrderPhotosCommand(id), cancellationToken));

    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Lista os pedidos do cliente, do mais recente para o mais antigo")]
    [ProducesResponseType<OrderCustomerPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] CustomerOrderFilter? status, [FromQuery] int? page, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ListCustomerOrdersQuery(status ?? CustomerOrderFilter.Open, page ?? 1), cancellationToken));

    [HttpGet("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Mostra o pedido com o orçamento, o agendamento e as ações permitidas")]
    [ProducesResponseType<OrderCustomerView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetCustomerOrderQuery(id), cancellationToken));

    [HttpGet("{id:guid}/suggestions")]
    [EnableRateLimiting(RateLimitPolicies.Search)]
    [EndpointSummary("Sugere até 3 profissionais parecidos para um pedido recusado ou sem resposta")]
    [ProducesResponseType<IReadOnlyList<ProviderSearchItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Suggestions(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetOrderSuggestionsQuery(id), cancellationToken));

    [HttpPost("{id:guid}/quote/accept")]
    [EndpointSummary("Aceita o orçamento e libera o endereço ao profissional")]
    [ProducesResponseType<OrderCustomerView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcceptQuote(Guid id, AcceptQuoteRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(id), cancellationToken));

    [HttpPost("{id:guid}/quote/decline")]
    [EndpointSummary("Recusa o orçamento")]
    [ProducesResponseType<OrderCustomerView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeclineQuote(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new DeclineQuoteCommand(id), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    [EndpointSummary("Cancela o pedido com um motivo")]
    [ProducesResponseType<OrderCustomerView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancelOrderRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCustomerCommand(id), cancellationToken));

    [HttpPost("{id:guid}/complete")]
    [EndpointSummary("Marca o serviço como concluído")]
    [ProducesResponseType<OrderCustomerView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new CompleteCustomerOrderCommand(id), cancellationToken));
}
