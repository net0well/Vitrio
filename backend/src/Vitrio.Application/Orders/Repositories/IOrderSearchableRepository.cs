using Vitrio.Application.Orders.Common;

namespace Vitrio.Application.Orders.Repositories;

public interface IOrderSearchableRepository
{
    Task<OrderCustomerView?> GetCustomerViewAsync(Guid orderId, Guid customerId, DateTimeOffset now, CancellationToken cancellationToken);

    Task<OrderCustomerPage> ListForCustomerAsync(Guid customerId, bool open, int skip, int take, CancellationToken cancellationToken);

    Task<OrderStoredPhotos?> FindStoredPhotosAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken);

    Task<OrderSuggestionContext?> FindSuggestionContextAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken);

    Task<OrderProviderView?> GetProviderViewAsync(Guid orderId, Guid providerId, DateTimeOffset now, CancellationToken cancellationToken);

    Task<OrderProviderPage> ListForProviderAsync(
        Guid providerId,
        ProviderOrderTab tab,
        DateTimeOffset now,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AgendaItemResponse>> ListAgendaAsync(
        Guid providerId,
        DateOnly from,
        DateOnly to,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<ProviderOrderNumbers> GetProviderNumbersAsync(
        Guid providerId,
        DateTimeOffset monthStart,
        DateTimeOffset nextMonthStart,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
