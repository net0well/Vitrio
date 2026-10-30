using Vitrio.Domain.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Domain.Providers;

public sealed class ProviderService
{
    public const int TitleMinLength = 2;
    public const int TitleMaxLength = 60;
    public const int DescriptionMaxLength = 140;
    public const long MinPriceCents = 100;
    public const long MaxPriceCents = 10_000_000;

    private ProviderService()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProviderId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public long? PriceFromCents { get; private set; }

    public PriceUnit PriceUnit { get; private set; }

    public bool OnQuote { get; private set; }

    public int SortOrder { get; private set; }

    internal static Result<ProviderService> Create(Guid providerId, ServiceDraft draft, int sortOrder)
    {
        var title = FreeText.Normalize(draft.Title);
        var description = FreeText.Normalize(draft.Description);
        if (title.Length is < TitleMinLength or > TitleMaxLength || description.Length > DescriptionMaxLength || !Enum.IsDefined(draft.Unit))
        {
            return ProviderErrors.InvalidServices;
        }

        if (FreeText.ContainsContactInformation(title) || FreeText.ContainsContactInformation(description))
        {
            return ProviderErrors.ContactInText;
        }

        var priced = draft.PriceFrom is { Cents: >= MinPriceCents and <= MaxPriceCents };
        if (draft.OnQuote == (draft.PriceFrom is not null) || (!draft.OnQuote && !priced))
        {
            return ProviderErrors.InvalidPrice;
        }

        return new ProviderService
        {
            Id = Guid.CreateVersion7(),
            ProviderId = providerId,
            Title = title,
            Description = description.Length == 0 ? null : description,
            PriceFromCents = draft.PriceFrom?.Cents,
            PriceUnit = draft.Unit,
            OnQuote = draft.OnQuote,
            SortOrder = sortOrder
        };
    }
}
