namespace Vitrio.Domain.Providers;

public sealed class ProviderCategory
{
    private ProviderCategory()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProviderId { get; private set; }

    public int CategoryId { get; private set; }

    public bool IsPrimary { get; private set; }

    internal static ProviderCategory Create(Guid providerId, int categoryId, bool isPrimary) => new()
    {
        Id = Guid.CreateVersion7(),
        ProviderId = providerId,
        CategoryId = categoryId,
        IsPrimary = isPrimary
    };
}
