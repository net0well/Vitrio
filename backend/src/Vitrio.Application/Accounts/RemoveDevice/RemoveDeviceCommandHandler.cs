using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.RemoveDevice;

public sealed class RemoveDeviceCommandHandler(ICurrentUser currentUser, IDeviceWritableRepository devices, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveDeviceCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveDeviceCommand command, CancellationToken cancellationToken)
    {
        var device = await devices.GetAsync(command.InstallationId, cancellationToken);
        if (device is not null && device.UserId == currentUser.Id)
        {
            devices.Remove(device);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
