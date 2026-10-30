namespace Vitrio.Application.Providers.Common;

public sealed record SharePageResponse(
    bool Found,
    string PlayStoreUrl,
    string? Url,
    string? DisplayName,
    string? Headline,
    string? Place,
    string? RatingText,
    string? ImageUrl);
