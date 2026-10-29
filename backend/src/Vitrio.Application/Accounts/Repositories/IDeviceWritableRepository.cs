using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IDeviceWritableRepository
{
    Task<Device?> GetAsync(string installationId, CancellationToken cancellationToken);

    void Add(Device device);

    void Remove(Device device);
}
