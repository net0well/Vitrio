using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Auditing.Repositories;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Application.Search.Repositories;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal static class RepositoriesRegistration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserWritableRepository, UserWritableRepository>();
        services.AddScoped<IUserSearchableRepository, UserSearchableRepository>();
        services.AddScoped<IEmailCodeWritableRepository, EmailCodeWritableRepository>();
        services.AddScoped<ISessionWritableRepository, SessionWritableRepository>();
        services.AddScoped<ISessionSearchableRepository, SessionSearchableRepository>();
        services.AddScoped<IRefreshTokenWritableRepository, RefreshTokenWritableRepository>();
        services.AddScoped<IAddressWritableRepository, AddressWritableRepository>();
        services.AddScoped<IAddressSearchableRepository, AddressSearchableRepository>();
        services.AddScoped<IDeviceWritableRepository, DeviceWritableRepository>();
        services.AddScoped<IAdminGrantWritableRepository, AdminGrantWritableRepository>();
        services.AddScoped<IAdminGrantSearchableRepository, AdminGrantSearchableRepository>();
        services.AddScoped<IAuditEventWritableRepository, AuditEventWritableRepository>();
        services.AddScoped<ICitySearchableRepository, CitySearchableRepository>();
        services.AddScoped<ICityWritableRepository, CityWritableRepository>();
        services.AddScoped<ICategorySearchableRepository, CategorySearchableRepository>();
        services.AddScoped<IProviderWritableRepository, ProviderWritableRepository>();
        services.AddScoped<IProviderSearchableRepository, ProviderSearchableRepository>();
        services.AddScoped<IProviderListingSearchableRepository, ProviderListingSearchableRepository>();
        return services;
    }
}
