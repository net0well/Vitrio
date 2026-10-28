using System.Reflection;
using System.Text.Json;
using Vitrio.Domain.Common;

namespace Vitrio.Domain.Tests.Common;

public sealed class DomainEventContractTests
{
    private static readonly Guid SampleId = Guid.Parse("0193c9a8-0000-7000-8000-000000000001");
    private static readonly DateTimeOffset SampleInstant = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Serialize_UsesTheReferenceShape()
    {
        var json = Serialize(new SampleEvent(SampleId, SampleCode.Second, null));

        Assert.Equal(
            """{"orderId":"0193c9a8-0000-7000-8000-000000000001","code":"Second","note":null,"eventId":"0193c9a8-0000-7000-8000-000000000001"}""",
            json);
    }

    [Fact]
    public void EveryDomainEvent_MatchesItsReferenceContract()
    {
        var problems = new List<string>();
        var eventTypes = typeof(DomainEvent).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsAssignableTo(typeof(DomainEvent)));

        foreach (var eventType in eventTypes)
        {
            var descriptor = eventType.GetCustomAttribute<EventTypeAttribute>();
            if (descriptor is null)
            {
                problems.Add($"{eventType.Name} has no [EventType].");
                continue;
            }

            var fileName = $"{descriptor.Name}.v{descriptor.Version}.json";
            var referencePath = Path.Combine(AppContext.BaseDirectory, "Contracts", fileName);
            var actual = Serialize(CreateSample(eventType));
            if (!File.Exists(referencePath))
            {
                problems.Add($"Add Contracts/{fileName} with: {actual}");
            }
            else if (File.ReadAllText(referencePath).Trim() != actual)
            {
                problems.Add($"{fileName} changed. If the change is compatible, update the file; if not, create version {descriptor.Version + 1}. Now: {actual}");
            }
        }

        Assert.Empty(problems);
    }

    private static string Serialize(DomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent with { EventId = SampleId }, domainEvent.GetType(), DomainEventJson.Options);

    private static DomainEvent CreateSample(Type eventType)
    {
        var constructor = eventType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        var arguments = constructor.GetParameters().Select(parameter => SampleValue(parameter.ParameterType)).ToArray();
        return (DomainEvent)constructor.Invoke(arguments);
    }

    private static object? SampleValue(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying switch
        {
            _ when underlying == typeof(Guid) => SampleId,
            _ when underlying == typeof(int) => 1,
            _ when underlying == typeof(bool) => true,
            _ when underlying == typeof(string) => "sample",
            _ when underlying == typeof(DateTimeOffset) => SampleInstant,
            { IsEnum: true } => Enum.GetValues(underlying).GetValue(0),
            _ => throw new InvalidOperationException($"Add a sample value for {underlying.Name}.")
        };
    }

    [EventType("tests.sample", 1)]
    private sealed record SampleEvent(Guid OrderId, SampleCode Code, string? Note) : DomainEvent;

    private enum SampleCode
    {
        First,
        Second
    }
}
