using System.Collections.Concurrent;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.ProcessAvatar;
using Vitrio.Application.Providers.ProcessPhoto;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class RecordingPhotoQueue : IPhotoProcessingQueue
{
    private readonly ConcurrentQueue<object> _pending = new();

    public void EnqueuePhoto(Guid providerId, Guid photoId) => _pending.Enqueue(new ProcessPhotoCommand(providerId, photoId));

    public void EnqueueAvatar(Guid providerId, string uploadKey) => _pending.Enqueue(new ProcessAvatarCommand(providerId, uploadKey));

    public async Task RunPendingAsync(IServiceProvider services)
    {
        while (_pending.TryDequeue(out var command))
        {
            await using var scope = services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, TestContext.Current.CancellationToken);
        }
    }
}
