using Npgsql;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Search;

internal sealed class ProviderListingSql
{
    private const string CityReachMeters = "50000";
    private const string PerService = nameof(PriceUnit.PerService);

    private const string AvailableTodaySql =
        "(p.available_today and p.available_today_set_at is not null " +
        "and (p.available_today_set_at at time zone c.time_zone)::date = (@now at time zone c.time_zone)::date)";

    private const string CityBoundarySql = "(select city_area.boundary from cities city_area where city_area.ibge_code = @city)";

    private static readonly string RatedSql = $"(p.review_count >= {RatingRule.MinReviews} and p.rating_score is not null)";

    private readonly List<string> _joins = [];
    private readonly List<string> _conditions = [];
    private readonly List<NpgsqlParameter> _parameters = [];
    private string _distance = "null::double precision";
    private string _savedAt = "null::timestamptz";
    private string _exactMatch = "true";
    private string? _text;
    private string? _proximity;

    private ProviderListingSql()
    {
    }

    public static ProviderListingStatement Search(ProviderListingCriteria criteria, SearchRelevanceOptions weights)
    {
        var sql = new ProviderListingSql();
        sql.Add("now", criteria.Now);
        sql.Area(criteria.Area, criteria.MaxDistanceMeters, 0);
        sql.Filters(criteria);
        return sql.Listing(OrderBy(criteria.Sort), weights, criteria.Skip, criteria.Take);
    }

    public static ProviderListingStatement Count(ProviderListingCriteria criteria, int limit)
    {
        var sql = new ProviderListingSql();
        sql.Add("now", criteria.Now);
        sql.Area(criteria.Area, criteria.MaxDistanceMeters, 0);
        sql.Filters(criteria);
        sql.Add("limit", limit);
        return new ProviderListingStatement($"select count(*)::int as total from (select 1 {sql.Source()} limit @limit) counted", sql._parameters);
    }

    public static ProviderListingStatement TopRated(SearchArea area, double marginMeters, int take, DateTimeOffset now, SearchRelevanceOptions weights)
    {
        var sql = new ProviderListingSql();
        sql.Add("now", now);
        sql.Area(area, SearchDistances.DefaultMaxMeters, marginMeters);
        sql._conditions.Add(RatedSql);
        return sql.Listing("rating_score desc, review_count desc, id", weights, 0, take);
    }

    public static ProviderListingStatement Favorites(Guid userId, int skip, int take, DateTimeOffset now, SearchRelevanceOptions weights)
    {
        var sql = new ProviderListingSql();
        sql.Add("now", now);
        sql.Add("user", userId);
        sql._joins.Add("join favorites favorite on favorite.provider_id = p.id and favorite.user_id = @user");
        sql._savedAt = "favorite.created_at";
        return sql.Listing("saved_at desc, id", weights, skip, take);
    }

    public static ProviderListingStatement Exists(SearchArea area)
    {
        var sql = new ProviderListingSql();
        sql.Area(area, SearchDistances.DefaultMaxMeters, 0);
        return new ProviderListingStatement($"select exists (select 1 {sql.Source()}) as found", sql._parameters);
    }

    private static string OrderBy(SearchSort sort) => sort switch
    {
        SearchSort.Distance => "distance_meters, relevance desc, id",
        SearchSort.Rating =>
            $"case when review_count >= {RatingRule.MinReviews} then rating_score end desc nulls last, review_count desc, relevance desc, id",
        SearchSort.Price => "service_price_cents nulls last, relevance desc, id",
        _ => "exact_match desc, relevance desc, id"
    };

    private static string MeiVerified(string providerId) =>
        $"exists (select 1 from mei_verifications mei where mei.provider_id = {providerId} and mei.status = '{nameof(MeiStatus.Verified)}')";

    private void Area(SearchArea area, double maxDistanceMeters, double marginMeters)
    {
        if (area is { IsNearMe: true, Point: { } point })
        {
            Add("lat", point.Latitude);
            Add("lng", point.Longitude);
            Add("max_distance", maxDistanceMeters + marginMeters);
            Add("margin", marginMeters);
            _joins.Add("cross join (select st_setsrid(st_makepoint(@lng, @lat), 4326)::geography as point) pt");
            _conditions.Add("st_dwithin(p.public_location, pt.point, @max_distance)");
            _conditions.Add("st_dwithin(p.public_location, pt.point, p.service_radius_meters + @margin)");
            _distance = "st_distance(p.public_location, pt.point)";
            _proximity = "greatest(0.0, 1 - st_distance(p.public_location, pt.point) / @max_distance)";
            return;
        }

        Add("city", area.City!.IbgeCode);
        _conditions.Add($"(p.city_ibge_code = @city or st_dwithin(p.public_location, {CityBoundarySql}, {CityReachMeters}))");
        _conditions.Add($"(p.city_ibge_code = @city or st_dwithin(p.public_location, {CityBoundarySql}, p.service_radius_meters, false))");
    }

    private void Filters(ProviderListingCriteria criteria)
    {
        if (criteria.Term is { } term)
        {
            Add("term", term);
            _joins.Add($"cross join (select websearch_to_tsquery('{ProviderSearchIndex.TextConfiguration}', @term) as tsq) q");
            _conditions.Add("(numnode(q.tsq) = 0 or p.search_vector @@ q.tsq or @term <% p.search_terms)");
            _exactMatch = "(numnode(q.tsq) = 0 or p.search_vector @@ q.tsq)";
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
            _conditions.Add(MeiVerified("p.id"));
        }

        if (criteria.AvailableToday)
        {
            _conditions.Add(AvailableTodaySql);
        }

        if (criteria.MaxPriceFromCents is { } maxPrice)
        {
            Add("max_price", maxPrice);
            _conditions.Add("p.search_service_price_cents <= @max_price");
        }
    }

    private string Source()
    {
        var joins = string.Join('\n', _joins);
        var conditions = string.Concat(_conditions.Select(condition => $"\n  and {condition}"));

        // O planejador subestima as vitrines que sobram dos filtros e, num join comum, varre a tabela de cidades
        // (com os limites dos municípios) uma vez por vitrine; o offset 0 mantém a cidade como busca pela chave primária.
        return $"""
            from providers p
            join users u on u.id = p.user_id
            cross join lateral (select city.name, city.time_zone from cities city where city.ibge_code = p.city_ibge_code offset 0) c
            {joins}
            where p.status = '{nameof(ProviderStatus.Published)}' and u.status = '{nameof(UserStatus.Active)}'{conditions}
            """;
    }

    private ProviderListingStatement Listing(string orderBy, SearchRelevanceOptions weights, int skip, int take)
    {
        var relevance = Relevance(weights);
        Add("skip", skip);
        Add("take", take);
        var sql = $"""
            select ranked.*, {MeiVerified("ranked.id")} as mei_verified, price.price_from_cents, price.price_unit
            from (
              select
                p.id,
                p.display_name,
                p.headline,
                p.avatar_key,
                p.rating_score,
                p.review_count,
                p.response_time_bucket,
                p.public_latitude,
                p.public_longitude,
                p.service_radius_meters,
                p.available_today as available_today_switch,
                p.available_today_set_at,
                p.search_service_price_cents as service_price_cents,
                c.name as city_name,
                c.time_zone,
                {_distance} as distance_meters,
                {_savedAt} as saved_at,
                {_exactMatch} as exact_match,
                {AvailableTodaySql} as available_today,
                ({relevance})::double precision as relevance
              {Source()}
              order by {orderBy}
              limit @take offset @skip
            ) ranked
            left join lateral (
              select service.price_from_cents, service.price_unit
              from provider_services service
              where service.provider_id = ranked.id and service.price_from_cents is not null and not service.on_quote
              order by service.price_unit <> '{PerService}', service.price_from_cents
              limit 1) price on true
            order by {orderBy}
            """;
        return new ProviderListingStatement(sql, _parameters);
    }

    private string Relevance(SearchRelevanceOptions weights)
    {
        List<(double Weight, string Sql)> components =
        [
            (weights.Rating, $"case when {RatedSql} then (p.rating_score - 1) / 4.0 else 0.5 end"),
            (weights.Reviews, "least(1.0, ln(1 + p.review_count) / ln(101))"),
            (weights.Completeness, "p.search_completeness"),
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
