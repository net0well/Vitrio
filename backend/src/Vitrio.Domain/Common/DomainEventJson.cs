using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vitrio.Domain.Common;

public static class DomainEventJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
