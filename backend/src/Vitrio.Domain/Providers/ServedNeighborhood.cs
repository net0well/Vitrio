using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

public sealed class ServedNeighborhood
{
    public const int MinLength = 2;
    public const int MaxLength = 60;

    private ServedNeighborhood()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProviderId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NameNormalized { get; private set; } = string.Empty;

    internal static ServedNeighborhood? Create(Guid providerId, string? name)
    {
        var normalized = FreeText.Normalize(name);
        if (normalized.Length is < MinLength or > MaxLength || FreeText.ContainsContactInformation(normalized))
        {
            return null;
        }

        return new ServedNeighborhood
        {
            Id = Guid.CreateVersion7(),
            ProviderId = providerId,
            Name = normalized,
            NameNormalized = SearchText.Normalize(normalized)
        };
    }
}
