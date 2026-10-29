using System.ComponentModel.DataAnnotations;

namespace Vitrio.Infrastructure.External;

public sealed class ExternalServicesOptions
{
    public const string SectionName = "ExternalServices";

    [Required]
    public Uri BrasilApiBaseUrl { get; init; } = new("https://brasilapi.com.br/api/");

    [Required]
    public Uri IbgeBaseUrl { get; init; } = new("https://servicodados.ibge.gov.br/api/");
}
