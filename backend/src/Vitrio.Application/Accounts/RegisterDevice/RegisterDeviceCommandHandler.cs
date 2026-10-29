using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.RegisterDevice;

public sealed class RegisterDeviceCommandHandler(
    ICurrentUser currentUser,
    IDeviceWritableRepository devices,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RegisterDeviceCommand, Result>
{
    public async ValueTask<Result> Handle(RegisterDeviceCommand command, CancellationToken cancellationToken)
    {
        var device = await devices.GetAsync(command.InstallationId, cancellationToken);
        if (device is null)
        {
            devices.Add(Device.Register(command.InstallationId, currentUser.Id, command.Platform, command.PushToken, clock));
        }
        else
        {
            device.Assign(currentUser.Id, command.Platform, command.PushToken, clock);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
