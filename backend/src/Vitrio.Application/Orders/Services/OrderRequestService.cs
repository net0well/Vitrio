using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.CreateOrder;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Orders;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.Services;

internal sealed class OrderRequestService(
    IUserSearchableRepository users,
    IProviderSearchableRepository providers,
    IOrderWritableRepository orders,
    IAddressWritableRepository addresses,
    ICitySearchableRepository cities,
    IObjectStorage storage) : IOrderRequestService
{
    private const string OtherService = "Outro";

    public async Task<Result<OrderRequest>> PrepareAsync(Guid customerId, CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var customer = await users.GetMeAsync(customerId, cancellationToken);
        if (customer?.PhoneNumber is not { } phone)
        {
            return OrderErrors.PhoneRequired;
        }

        var service = ServiceFor(await providers.FindOrderTargetAsync(command.ProviderId, cancellationToken), customerId, command.ServiceId);
        if (service.IsFailure)
        {
            return service.Error;
        }

        var (serviceTitle, categoryId) = service.Value;
        var limits = OrderLimits.Check(
            await orders.CountOpenForCustomerAsync(customerId, cancellationToken),
            await orders.CountWaitingForCustomerAsync(customerId, categoryId, cancellationToken));
        if (limits.IsFailure)
        {
            return limits.Error;
        }

        var address = await addresses.GetForUserAsync(command.AddressId, customerId, cancellationToken);
        if (address is null)
        {
            return AccountErrors.AddressNotFound;
        }

        var place = await PlaceOfAsync(address, cancellationToken);
        if (place.IsFailure)
        {
            return place.Error;
        }

        var photos = await VerifyUploadsAsync(customerId, command.PhotoKeys, cancellationToken);
        if (photos.IsFailure)
        {
            return photos.Error;
        }

        return new OrderRequest(
            command.ServiceId,
            serviceTitle,
            categoryId,
            command.Title,
            command.Description,
            command.PreferredDay,
            command.PreferredPeriod!.Value,
            new OrderAddress(address.Street, address.Number, address.Complement, address.Cep),
            new OrderContact(customer.DisplayName, phone),
            place.Value,
            command.PhotoKeys);
    }

    private static Result<(string Title, int CategoryId)> ServiceFor(ProviderOrderTarget? provider, Guid customerId, Guid? serviceId)
    {
        if (provider is null || !provider.IsVisible)
        {
            return ProviderErrors.NotFound;
        }

        if (provider.UserId == customerId)
        {
            return OrderErrors.OwnProvider;
        }

        if (!provider.ReceivesOrders || provider.PrimaryCategoryId is not { } categoryId)
        {
            return OrderErrors.ProviderNotReceiving;
        }

        if (serviceId is not { } id)
        {
            return (OtherService, categoryId);
        }

        return provider.Services.TryGetValue(id, out var title) ? (title, categoryId) : OrderErrors.ServiceNotFound;
    }

    private async Task<Result<OrderPlace>> PlaceOfAsync(Address address, CancellationToken cancellationToken)
    {
        var city = await cities.GetAsync(address.CityIbgeCode, cancellationToken);
        if (city is null)
        {
            return LocationErrors.CityNotFound;
        }

        var point = Coordinates.InBrazil(address.Latitude, address.Longitude);
        return new OrderPlace(
            city.IbgeCode,
            city.Name,
            city.State,
            address.Neighborhood,
            city.TimeZone,
            point?.Latitude ?? city.Centroid.Lat,
            point?.Longitude ?? city.Centroid.Lng,
            point is null);
    }

    private async Task<Result> VerifyUploadsAsync(Guid customerId, IReadOnlyList<string> keys, CancellationToken cancellationToken)
    {
        foreach (var key in keys)
        {
            if (!OrderPhotoKeys.BelongsTo(key, customerId) || await storage.FindUploadAsync(key, cancellationToken) is null)
            {
                return OrderErrors.PhotoUploadNotFound;
            }
        }

        return Result.Success();
    }
}
