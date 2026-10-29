using System.Reflection;
using Vitrio.Application.Events;
using Vitrio.Domain.Common;

namespace Vitrio.Infrastructure.Events;

public sealed class DomainEventRegistry
{
    private readonly Dictionary<Type, EventTypeAttribute> _descriptors = [];
    private readonly Dictionary<(string Name, int Version), Type> _eventTypes = [];
    private readonly Dictionary<Type, List<string>> _handlersByEvent = [];
    private readonly Dictionary<string, Type> _handlerTypes = new(StringComparer.Ordinal);

    public DomainEventRegistry(IEnumerable<Type> candidateTypes)
    {
        var types = candidateTypes.Distinct().ToList();

        foreach (var eventType in types.Where(IsConcreteEvent))
        {
            var descriptor = eventType.GetCustomAttribute<EventTypeAttribute>(inherit: false)
                ?? throw new InvalidOperationException($"Domain event '{eventType.FullName}' has no [EventType].");

            if (!_eventTypes.TryAdd((descriptor.Name, descriptor.Version), eventType))
            {
                throw new InvalidOperationException(
                    $"Event type '{descriptor.Name}' v{descriptor.Version} is declared by more than one class.");
            }

            _descriptors[eventType] = descriptor;
        }

        foreach (var handlerType in types.Where(type => type is { IsClass: true, IsAbstract: false }))
        {
            var handledEvents = handlerType.GetInterfaces()
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .Select(contract => contract.GetGenericArguments()[0])
                .ToList();

            if (handledEvents.Count == 0)
            {
                continue;
            }

            if (_handlerTypes.TryGetValue(handlerType.Name, out var existing) && existing != handlerType)
            {
                throw new InvalidOperationException($"Two event handlers are named '{handlerType.Name}'.");
            }

            _handlerTypes[handlerType.Name] = handlerType;
            foreach (var eventType in handledEvents)
            {
                if (!_handlersByEvent.TryGetValue(eventType, out var handlers))
                {
                    _handlersByEvent[eventType] = handlers = [];
                }

                handlers.Add(handlerType.Name);
            }
        }
    }

    public IReadOnlyCollection<Type> HandlerTypes => _handlerTypes.Values;

    public EventTypeAttribute Describe(Type eventType) =>
        _descriptors.TryGetValue(eventType, out var descriptor)
            ? descriptor
            : throw new InvalidOperationException($"Domain event '{eventType.FullName}' is not registered.");

    public Type EventType(string name, int version) =>
        _eventTypes.TryGetValue((name, version), out var eventType)
            ? eventType
            : throw new InvalidOperationException($"Event type '{name}' v{version} is not registered.");

    public IReadOnlyList<string> HandlersFor(Type eventType) =>
        _handlersByEvent.TryGetValue(eventType, out var handlers) ? handlers : [];

    public Type HandlerType(string handler) =>
        _handlerTypes.TryGetValue(handler, out var handlerType)
            ? handlerType
            : throw new InvalidOperationException($"Event handler '{handler}' is not registered.");

    private static bool IsConcreteEvent(Type type) =>
        type is { IsClass: true, IsAbstract: false } && type.IsAssignableTo(typeof(DomainEvent));
}
