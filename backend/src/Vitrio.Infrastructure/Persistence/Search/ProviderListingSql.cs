using Npgsql;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Search;

internal sealed class ProviderListingSql
{
    private const string MinWordSimilarity = "0.5";
    private const string CityReachMeters = "50000";
    private const string PerService = nameof(PriceUnit.PerService);

    private const string AvailableTodaySql =
        "(p.available_today and p.available_today_set_at is not null " +
        "and (p.available_today_set_at at time zone c.time_zone)::date = (@now at time zone c.time_zone)::date)";

    private const string CompletenessSql =
        "((p.avatar_key is not null)::int " +
        $"+ ((select count(*) from portfolio_photos photo where photo.provider_id = p.id and photo.status = '{nameof(PhotoStatus.Active)}') >= 3)::int " +
        "+ (exists (select 1 from availability_slots slot where slot.provider_id = p.id))::int " +
        "+ (p.years_of_experience is not null)::int) / 4.0";

    private static readonly string RatedSql = $"(p.review_count >= {RatingRule.MinReviews} and p.rating_score is not null)";

    private readonly List<string> _joins = [];
    private readonly List<string> _conditions = [];
    private readonly List<NpgsqlParameter> _parameters = [];
    private string _distance = "null::double precision";
    private string? _text;
    private string? _proximity;

    private ProviderListingSql()
    {
    }

    public static ProviderListingStatement Search(ProviderListingCriteria criteria, SearchRelevanceOptions weights)
    {
        var sql = new ProviderListingSql();
        sql.Add("now", criteria.Now);
        sql.Area(criteria.Area, criteria.MaxDistanceMeters);
        sql.Filters(criteria);
        return sql.Build(criteria.Sort, weights, criteria.Skip, criteria.Take);
    }

    private static string OrderBy(SearchSort sort) => sort switch
    {
        SearchSort.Distance => "distance_meters, relevance desc, id",
        SearchSort.Rating =>
            $"case when review_count >= {RatingRule.MinReviews} then rating_score end desc nulls last, review_count desc, relevance desc, id",
        SearchSort.Price =>
            $"case when price_unit = '{PerService}' then price_from_cents end nulls last, relevance desc, id",
        _ => "relevance desc, id"
    };

    private void Area(SearchArea area, double maxDistanceMeters)
    {
        if (area is { IsNearMe: true, Point: { } point })
        {
            Add("lat", point.Latitude);
            Add("lng", point.Longitude);
            Add("max_distance", maxDistanceMeters);
            _joins.Add("cross join (select st_setsrid(st_makepoint(@lng, @lat), 4326)::geography as point) pt");
            _conditions.Add("st_dwithin(p.public_location, pt.point, @max_distance)");
            _conditions.Add("st_dwithin(p.public_location, pt.point, p.service_radius_meters)");
            _distance = "st_distance(p.public_location, pt.point)";
            _proximity = "greatest(0.0, 1 - st_distance(p.public_location, pt.point) / @max_distance)";
            return;
        }

        Add("city", area.City!.IbgeCode);
        _joins.Add("cross join (select boundary from cities where ibge_code = @city) area");
        _conditions.Add(
            "(p.city_ibge_code = @city " +
            $"or (st_dwithin(p.public_location, area.boundary, {CityReachMeters}) and st_dwithin(p.public_location, area.boundary, p.service_radius_meters)))");
    }

    private void Filters(ProviderListingCriteria criteria)
    {
        if (criteria.Term is { } term)
        {
            Add("term", term);
            _joins.Add($"cross join (select websearch_to_tsquery('{ProviderSearchIndex.TextConfiguration}', @term) as tsq) q");
            _conditions.Add(
                "(numnode(q.tsq) = 0 or p.search_vector @@ q.tsq " +
                $"or word_similarity(@term, coalesce(p.search_terms, '')) >= {MinWordSimilarity})");
            _text =
                "case when numnode(q.tsq) = 0 then 0 " +
                "else greatest(ts_rank(p.search_vector, q.tsq, 32), word_similarity(@term, coalesce(p.search_terms, ''))) end";
        }

        if (criteria.CategoryId is { } categoryId)
        {
            Add("category", categoryId);
            _conditions.Add("exists (select 1 from provider_categories chosen where chosen.provider_id = p.id and chosen.category_id = @category)");
        }

        if (criteria.MinRating is { } minRating)
        {
            Add("min_rating", minRating);
            _conditions.Add($"{RatedSql} and p.rating_score >= @min_rating");
        }

        if (criteria.MeiVerified)
        {
            _conditions.Add($"m.status = '{nameof(MeiStatus.Verified)}'");
        }

        if (criteria.AvailableToday)
        {
            _conditions.Add(AvailableTodaySql);
        }

        if (criteria.MaxPriceFromCents is { } maxPrice)
        {
            Add("max_price", maxPrice);
            _conditions.Add($"price.price_unit = '{PerService}' and price.price_from_cents <= @max_price");
        }
    }

    private ProviderListingStatement Build(SearchSort sort, SearchRelevanceOptions weights, int skip, int take)
    {
        Add("skip", skip);
        Add("take", take);
        var relevance = Relevance(weights);
        var joins = string.Join('\n', _joins);
        var conditions = string.Concat(_conditions.Select(condition => $"\n  and {condition}"));
        var sql = $"""
            select * from (
              select
                p.id,
                p.display_name,
                p.headline,
                p.avatar_key,
                p.rating_score,
                p.review_count,
                p.response_time_bucket,
                c.name as city_name,
                {_distance} as distance_meters,
                coalesce(m.status = '{nameof(MeiStatus.Verified)}', false) as mei_verified,
                {AvailableTodaySql} as available_today,
                price.price_from_cents,
                price.price_unit,
                ({relevance})::double precision as relevance
              from providers p
              join users u on u.id = p.user_id
              join cities c on c.ibge_code = p.city_ibge_code
              left join mei_verifications m on m.provider_id = p.id
              left join lateral (
                select service.price_from_cents, service.price_unit
                from provider_services service
                where service.provider_id = p.id and service.price_from_cents is not null and not service.on_quote
                order by service.price_unit <> '{PerService}', service.price_from_cents
                limit 1) price on true
              {joins}
              where p.status = '{nameof(ProviderStatus.Published)}' and u.status = '{nameof(UserStatus.Active)}'{conditions}
            ) listing
            order by {OrderBy(sort)}
            limit @take offset @skip
            """;
        return new ProviderListingStatement(sql, _parameters);
    }

    private string Relevance(SearchRelevanceOptions weights)
    {
        List<(double Weight, string Sql)> components =
        [
            (weights.Rating, $"case when {RatedSql} then (p.rating_score - 1) / 4.0 else 0.5 end"),
            (weights.Reviews, "least(1.0, ln(1 + p.review_count) / ln(101))"),
            (weights.Completeness, CompletenessSql),
            (weights.AvailableToday, $"{AvailableTodaySql}::int")
        ];
        if (_text is not null)
        {
            components.Add((weights.Text, _text));
        }

        if (_proximity is not null)
        {
            components.Add((weights.Proximity, _proximity));
        }

        var total = components.Sum(component => component.Weight);
        return string.Join(" + ", components.Select((component, index) =>
        {
            Add($"weight_{index}", total > 0 ? component.Weight / total : 0);
            return $"@weight_{index} * ({component.Sql})";
        }));
    }

    private void Add(string name, object value) => _parameters.Add(new NpgsqlParameter(name, value));
}
