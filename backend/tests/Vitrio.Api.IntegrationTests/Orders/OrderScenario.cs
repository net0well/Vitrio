using Vitrio.Api.IntegrationTests.Search;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed record OrderScenario(PublishedProvider Provider, OrderCustomer Customer, GeoSpot Spot);
