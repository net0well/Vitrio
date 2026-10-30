using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Accounts.Services;
using Vitrio.Application.Locations.Services;
using Vitrio.Application.Providers.Services;
using Vitrio.Application.Search.Services;

namespace Vitrio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<IEmailCodeService, EmailCodeService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IPasswordChangeService, PasswordChangeService>();
        services.AddScoped<IAddressLookupService, AddressLookupService>();
        services.AddScoped<IAccountRolesService, AccountRolesService>();
        services.AddScoped<IServiceAreaService, ServiceAreaService>();
        services.AddScoped<IMeiCheckService, MeiCheckService>();
        services.AddScoped<IPublicProfileService, PublicProfileService>();
        services.AddScoped<ICategoryListService, CategoryListService>();
        services.AddScoped<ICityLocationService, CityLocationService>();
        services.AddScoped<ISearchAreaService, SearchAreaService>();
        return services;
    }
}
