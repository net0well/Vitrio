using Vitrio.Api.IntegrationTests.Search;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed record OrderCustomer(string AccessToken, Guid Id, Guid AddressId, GeoSpot Point);
