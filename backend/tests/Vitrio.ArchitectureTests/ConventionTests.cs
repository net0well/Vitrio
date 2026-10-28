using Mediator;
using Microsoft.AspNetCore.Mvc;
using Vitrio.Api.Controllers;

namespace Vitrio.ArchitectureTests;

public sealed class ConventionTests
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>),
        typeof(IRequestHandler<,>),
        typeof(INotificationHandler<>)
    ];

    [Fact]
    public void Controllers_InheritApiControllerBase()
    {
        var violations = Controllers()
            .Where(controller => !controller.IsSubclassOf(typeof(ApiControllerBase)))
            .Select(controller => controller.Name);

        Assert.Empty(violations);
    }

    [Fact]
    public void Controllers_AreSealed()
    {
        var violations = Controllers()
            .Where(controller => !controller.IsSealed)
            .Select(controller => controller.Name);

        Assert.Empty(violations);
    }

    [Fact]
    public void Controllers_OnlyReceiveTheSender()
    {
        var violations = Controllers()
            .Where(controller => controller.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters())
                .Any(parameter => parameter.ParameterType != typeof(ISender)))
            .Select(controller => controller.Name);

        Assert.Empty(violations);
    }

    [Fact]
    public void Handlers_AreSealed()
    {
        var violations = Layers.Application.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.GetInterfaces().Any(IsHandlerInterface))
            .Where(type => !type.IsSealed)
            .Select(type => type.Name);

        Assert.Empty(violations);
    }

    private static IEnumerable<Type> Controllers() =>
        Layers.Api.GetTypes().Where(type =>
            type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(ControllerBase)));

    private static bool IsHandlerInterface(Type type) =>
        type.IsGenericType && HandlerInterfaces.Contains(type.GetGenericTypeDefinition());
}
