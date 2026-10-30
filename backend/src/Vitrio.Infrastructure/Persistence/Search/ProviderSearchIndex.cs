namespace Vitrio.Infrastructure.Persistence.Search;

internal static class ProviderSearchIndex
{
    public const string TextConfiguration = "portuguese";

    public const string Primary = "SearchPrimary";
    public const string Name = "SearchName";
    public const string Secondary = "SearchSecondary";
    public const string Terms = "SearchTerms";
    public const string Vector = "SearchVector";
    public const string Location = "PublicLocation";

    public const string VectorSql =
        $"setweight(to_tsvector('{TextConfiguration}'::regconfig, coalesce(search_primary, '')), 'A') || " +
        $"setweight(to_tsvector('{TextConfiguration}'::regconfig, coalesce(search_name, '')), 'B') || " +
        $"setweight(to_tsvector('{TextConfiguration}'::regconfig, coalesce(search_secondary, '')), 'C')";

    public const string LocationSql = "st_setsrid(st_makepoint(public_longitude, public_latitude), 4326)::geography";
}
