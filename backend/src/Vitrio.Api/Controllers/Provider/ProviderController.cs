using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.Authorization;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.GetProvider;
using Vitrio.Application.Providers.GetProviderDashboard;
using Vitrio.Application.Providers.PauseProvider;
using Vitrio.Application.Providers.PublishProvider;
using Vitrio.Domain.Providers;

namespace Vitrio.Api.Controllers.Provider;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/provider")]
[Authorize]
public sealed class ProviderController(ISender sender) : ApiControllerBase
{
    [HttpPost]
    [EndpointSummary("Cria a vitrine em rascunho")]
    [ProducesResponseType<ProviderPrivateResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateProviderRequest request, CancellationToken cancellationToken) =>
        ToCreatedResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpGet]
    [Authorize(Policy = Policies.Provider)]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Mostra a vitrine do prestador, com o que falta para publicar")]
    [ProducesResponseType<ProviderPrivateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetProviderQuery(), cancellationToken));

    [HttpPatch]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Altera os dados da vitrine")]
    [ProducesResponseType<ProviderPrivateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(UpdateProviderRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPut("categories")]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Define até 3 categorias, com uma principal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCategories(ProviderCategoriesRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPut("service-area")]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Define o endereço base, o raio e os bairros atendidos")]
    [ProducesResponseType<ServiceAreaSavedResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SetServiceArea(ServiceAreaRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPut("services")]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Define até 20 serviços com preço ou sob orçamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetServices(ProviderServicesRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPut("availability")]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Define os dias e as faixas de atendimento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvailability(AvailabilityRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPut("available-today")]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Liga ou desliga o \"Disponível hoje\"")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetAvailableToday(AvailableTodayRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("publish")]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Publica a vitrine ou tira da pausa")]
    [ProducesResponseType<ProviderStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Publish(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new PublishProviderCommand(), cancellationToken));

    [HttpPost("pause")]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Pausa a vitrine publicada")]
    [ProducesResponseType<ProviderStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pause(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new PauseProviderCommand(), cancellationToken));

    [HttpPost("mei-verification")]
    [Authorize(Policy = Policies.Provider)]
    [EndpointSummary("Verifica o CNPJ para o selo MEI; responde 202 quando a consulta fica pendente")]
    [ProducesResponseType<MeiVerificationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<MeiVerificationResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> VerifyMei(MeiVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result is { IsSuccess: true, Value.Status: MeiStatus.Pending } ? ToAcceptedResult(result) : ToActionResult(result);
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = Policies.Provider)]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Mostra o Painel do prestador")]
    [ProducesResponseType<ProviderDashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new GetProviderDashboardQuery(), cancellationToken));
}
