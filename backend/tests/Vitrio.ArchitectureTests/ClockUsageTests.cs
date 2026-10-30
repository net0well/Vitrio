using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Vitrio.ArchitectureTests;

public sealed class ClockUsageTests
{
    private static readonly HashSet<string> ForbiddenMembers =
    [
        "System.DateTime System.DateTime::get_Now()",
        "System.DateTime System.DateTime::get_UtcNow()",
        "System.DateTime System.DateTime::get_Today()",
        "System.DateTimeOffset System.DateTimeOffset::get_Now()",
        "System.DateTimeOffset System.DateTimeOffset::get_UtcNow()"
    ];

    [Fact]
    public void NoLayer_ReadsTheSystemClockDirectly()
    {
        var violations = Layers.All
            .SelectMany(FindForbiddenCalls)
            .ToList();

        Assert.Empty(violations);
    }

    private static IEnumerable<string> FindForbiddenCalls(Assembly assembly)
    {
        using var module = ModuleDefinition.ReadModule(assembly.Location);
        return module.GetTypes()
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions
                .Where(instruction => instruction.OpCode.FlowControl == FlowControl.Call
                    && instruction.Operand is MethodReference called
                    && ForbiddenMembers.Contains(called.FullName))
                .Select(_ => $"{method.DeclaringType.FullName}.{method.Name}"))
            .ToList();
    }
}
