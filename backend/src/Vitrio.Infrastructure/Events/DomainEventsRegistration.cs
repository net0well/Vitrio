using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vitrio.Application.Events;
using Vitrio.Domain.Common;

namespace Vitrio.Infrastructure.Events;

public static class DomainEventsRegistration
{
    public static IServiceCollection AddDomainEvents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OutboxOptions>().Bind(configuration.GetSection(OutboxOptions.SectionName));
        services.AddMetrics();
        services.AddDomainEventTypes(
            new[] { typeof(DomainEvent).Assembly, typeof(IDomainEventHandler<>).Assembly, typeof(DomainEventsRegistration).Assembly }
                .SelectMany(assembly => assembly.GetTypes()));

        services.TryAddSingleton(serviceProvider =>
            new DomainEventRegistry(serviceProvider.GetRequiredService<IOptions<DomainEventOptions>>().Value.Types));
        services.TryAddSingleton<OutboxMetrics>();
        services.TryAddSingleton<OutboxProcessor>();
        services.AddHostedService<OutboxDispatcher>();
        return services;
    }

    public static IServiceCollection AddDomainEventTypes(this IServiceCollection services, IEnumerable<Type> types)
    {
        var candidates = types.ToList();
        services.Configure<DomainEventOptions>(options => options.Types.UnionWith(candidates));

        var handlerTypes = candidates
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.GetInterfaces().Any(IsEventHandlerContract));

        foreach (var handlerType in handlerTypes)
        {
            services.TryAddScoped(handlerType);
        }

        return services;
    }

    private static bool IsEventHandlerContract(Type contract) =>
        contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>);
}
