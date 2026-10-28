using Serilog.Core;
using Serilog.Events;

namespace Vitrio.Api.Logging;

public sealed class SensitiveDataMaskingEnricher : ILogEventEnricher
{
    private const string Mask = "***";

    private static readonly HashSet<string> SensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Name", "FullName", "DisplayName", "Email", "Phone", "PhoneNumber", "WhatsApp",
        "Address", "Street", "Number", "Complement", "Neighborhood", "Cep", "PostalCode", "Cpf", "Cnpj",
        "Latitude", "Longitude", "Lat", "Lng", "Location", "Coordinates",
        "Token", "AccessToken", "RefreshToken", "IdToken", "Password", "Secret", "Authorization", "Nonce"
    };

    private static readonly HashSet<string> FrameworkProperties = new(StringComparer.Ordinal) { "EventId", "SourceContext" };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var (name, value) in logEvent.Properties.ToList())
        {
            if (FrameworkProperties.Contains(name))
            {
                continue;
            }

            var masked = SensitiveNames.Contains(name) ? new ScalarValue(Mask) : MaskNested(value);
            if (!ReferenceEquals(masked, value))
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(name, masked));
            }
        }
    }

    private static LogEventPropertyValue MaskNested(LogEventPropertyValue value) => value switch
    {
        StructureValue structure => MaskStructure(structure),
        SequenceValue sequence => MaskSequence(sequence),
        DictionaryValue dictionary => MaskDictionary(dictionary),
        _ => value
    };

    private static LogEventPropertyValue MaskStructure(StructureValue structure)
    {
        var changed = false;
        var properties = new List<LogEventProperty>(structure.Properties.Count);
        foreach (var property in structure.Properties)
        {
            var masked = SensitiveNames.Contains(property.Name) ? new ScalarValue(Mask) : MaskNested(property.Value);
            changed |= !ReferenceEquals(masked, property.Value);
            properties.Add(ReferenceEquals(masked, property.Value) ? property : new LogEventProperty(property.Name, masked));
        }

        return changed ? new StructureValue(properties, structure.TypeTag) : structure;
    }

    private static LogEventPropertyValue MaskSequence(SequenceValue sequence)
    {
        var elements = sequence.Elements.Select(MaskNested).ToList();
        return elements.Where((element, index) => !ReferenceEquals(element, sequence.Elements[index])).Any()
            ? new SequenceValue(elements)
            : sequence;
    }

    private static LogEventPropertyValue MaskDictionary(DictionaryValue dictionary)
    {
        var changed = false;
        var entries = new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>(dictionary.Elements.Count);
        foreach (var (key, value) in dictionary.Elements)
        {
            var masked = key.Value is string name && SensitiveNames.Contains(name) ? new ScalarValue(Mask) : MaskNested(value);
            changed |= !ReferenceEquals(masked, value);
            entries.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>(key, masked));
        }

        return changed ? new DictionaryValue(entries) : dictionary;
    }
}
