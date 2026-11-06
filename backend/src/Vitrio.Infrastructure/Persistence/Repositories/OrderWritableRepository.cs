using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class OrderWritableRepository(AppDbContext db) : IOrderWritableRepository
{
    public Task<Order?> GetForCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken) =>
        db.Orders.Include(order => order.Photos)
            .SingleOrDefaultAsync(order => order.Id == orderId && order.CustomerId == customerId, cancellationToken);

    public Task<Order?> GetForProviderAsync(Guid orderId, Guid providerId, CancellationToken cancellationToken) =>
        db.Orders.Include(order => order.Photos)
            .SingleOrDefaultAsync(order => order.Id == orderId && order.ProviderId == providerId, cancellationToken);

    public Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Orders.Include(order => order.Photos).SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken);

    public void Add(Order order) => db.Orders.Add(order);
}
