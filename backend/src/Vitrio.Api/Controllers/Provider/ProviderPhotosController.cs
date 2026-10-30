using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vitrio.Api.Authorization;
using Vitrio.Api.RateLimiting;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.DeletePhoto;
using Vitrio.Application.Providers.ListProviderPhotos;
using Vitrio.Application.Providers.RemoveAvatar;

namespace Vitrio.Api.Controllers.Provider;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/provider")]
[Authorize(Policy = Policies.Provider)]
public sealed class ProviderPhotosController(ISender sender) : ApiControllerBase
{
    [HttpPost("photos/upload-url")]
    [EnableRateLimiting(RateLimitPolicies.Uploads)]
    [EndpointSummary("Gera o endereço para enviar uma foto direto ao storage")]
    [ProducesResponseType<PhotoUploadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateUpload(PhotoUploadRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpPost("photos")]
    [EndpointSummary("Confirma a foto enviada e manda processar")]
    [ProducesResponseType<PhotoAcceptedResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(ConfirmPhotoRequest request, CancellationToken cancellationToken) =>
        ToAcceptedResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpGet("photos")]
    [EnableRateLimiting(RateLimitPolicies.Profile)]
    [EndpointSummary("Lista as fotos da vitrine, inclusive em processamento e recusadas")]
    [ProducesResponseType<IReadOnlyList<ProviderPhotoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new ListProviderPhotosQuery(), cancellationToken));

    [HttpPatch("photos/{id:guid}")]
    [EndpointSummary("Altera a legenda de uma foto")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rename(Guid id, PhotoCaptionRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(id), cancellationToken));

    [HttpPut("photos/order")]
    [EndpointSummary("Reordena as fotos da vitrine")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Reorder(PhotoOrderRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpDelete("photos/{id:guid}")]
    [EndpointSummary("Apaga uma foto da vitrine")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new DeletePhotoCommand(id), cancellationToken));

    [HttpPut("avatar")]
    [EndpointSummary("Troca a foto de perfil pela foto enviada")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvatar(AvatarRequest request, CancellationToken cancellationToken) =>
        ToAcceptedResult(await sender.Send(request.ToCommand(), cancellationToken));

    [HttpDelete("avatar")]
    [EndpointSummary("Tira a foto de perfil")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveAvatar(CancellationToken cancellationToken) =>
        ToActionResult(await sender.Send(new RemoveAvatarCommand(), cancellationToken));
}
