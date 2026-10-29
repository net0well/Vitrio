using System.ComponentModel.DataAnnotations;

namespace Vitrio.Infrastructure.Links;

public sealed class PublicLinksOptions
{
    public const string SectionName = "PublicLinks";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://vitrio.app";
}
