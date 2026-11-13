using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderProviderCard(
    Guid Id,
    string DisplayName,
    string Headline,
    string Initials,
    string? AvatarUrl,
    ProviderRatingResponse? Rating,
    bool IsNew,
    string? WhatsAppPhone);
