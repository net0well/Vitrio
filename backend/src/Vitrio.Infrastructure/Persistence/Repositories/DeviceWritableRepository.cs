using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class DeviceWritableRepository(AppDbContext db) : IDeviceWritableRepository
{
    public Task<Device?> GetAsync(string installationId, CancellationToken cancellationToken) =>
        db.Devices.SingleOrDefaultAsync(device => device.InstallationId == installationId, cancellationToken);

    public void Add(Device device) => db.Devices.Add(device);

    public void Remove(Device device) => db.Devices.Remove(device);

    public async Task RemoveAllForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Devices.RemoveRange(await db.Devices.Where(device => device.UserId == userId).ToListAsync(cancellationToken));
}
