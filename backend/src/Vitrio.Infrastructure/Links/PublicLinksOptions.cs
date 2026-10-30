using System.ComponentModel.DataAnnotations;

namespace Vitrio.Infrastructure.Links;

public sealed class PublicLinksOptions
{
    public const string SectionName = "PublicLinks";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://vitrio.app";

    [Required]
    [Url]
    public string PlayStoreUrl { get; set; } = "https://play.google.com/store/apps/details?id=app.vitrio";

    [Required]
    public string AndroidPackage { get; set; } = "app.vitrio";

    public List<string> AndroidCertificateFingerprints { get; set; } = [];
}
