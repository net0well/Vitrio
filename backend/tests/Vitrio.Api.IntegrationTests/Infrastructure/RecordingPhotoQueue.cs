using System.Collections.Concurrent;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.ProcessOrderPhoto;
using Vitrio.Application.Providers.ProcessAvatar;
using Vitrio.Application.Providers.ProcessPhoto;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class RecordingPhotoQueue : IPhotoProcessingQueue
{
    private readonly ConcurrentQueue<object> _pending = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<Guid>> _orderPhotos = new();

    public void EnqueuePhoto(Guid providerId, Guid photoId) => _pending.Enqueue(new ProcessPhotoCommand(providerId, photoId));

    public void EnqueueAvatar(Guid providerId, string uploadKey) => _pending.Enqueue(new ProcessAvatarCommand(providerId, uploadKey));

    public void EnqueueOrderPhoto(Guid orderId, Guid photoId) => _orderPhotos.GetOrAdd(orderId, _ => new ConcurrentQueue<Guid>()).Enqueue(photoId);

    public bool HasOrderPhotos(Guid orderId) => _orderPhotos.TryGetValue(orderId, out var photos) && !photos.IsEmpty;

    public async Task RunPendingAsync(IServiceProvider services)
    {
        while (_pending.TryDequeue(out var command))
        {
            await SendAsync(services, command);
        }
    }

    public async Task RunOrderPhotosAsync(IServiceProvider services, Guid orderId)
    {
        if (!_orderPhotos.TryRemove(orderId, out var photos))
        {
            return;
        }

        while (photos.TryDequeue(out var photoId))
        {
            await SendAsync(services, new ProcessOrderPhotoCommand(orderId, photoId));
        }
    }

    private static async Task SendAsync(IServiceProvider services, object command)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, TestContext.Current.CancellationToken);
    }
}
