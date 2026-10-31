using System.ComponentModel.DataAnnotations;

namespace Vitrio.Infrastructure.Persistence.Search;

public sealed class SearchRelevanceOptions
{
    public const string SectionName = "Search:Relevance";

    [Range(0.0, 1.0)]
    public double Text { get; set; } = 0.35;

    [Range(0.0, 1.0)]
    public double Rating { get; set; } = 0.25;

    [Range(0.0, 1.0)]
    public double Reviews { get; set; } = 0.15;

    [Range(0.0, 1.0)]
    public double Proximity { get; set; } = 0.15;

    [Range(0.0, 1.0)]
    public double Completeness { get; set; } = 0.05;

    [Range(0.0, 1.0)]
    public double AvailableToday { get; set; } = 0.05;
}
