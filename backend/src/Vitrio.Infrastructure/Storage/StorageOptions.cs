using System.ComponentModel.DataAnnotations;

namespace Vitrio.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    [Required]
    [Url]
    public string ServiceUrl { get; set; } = string.Empty;

    [Url]
    public string? UploadServiceUrl { get; set; }

    [Required]
    public string AccessKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Region { get; set; } = "auto";

    [Required]
    public string UploadsBucket { get; set; } = "vitrio-uploads";

    [Required]
    public string PublicBucket { get; set; } = "vitrio-public";

    [Required]
    [Url]
    public string PublicBaseUrl { get; set; } = string.Empty;

    public bool CreateBuckets { get; set; }
}
