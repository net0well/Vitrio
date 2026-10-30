using Npgsql;

namespace Vitrio.Infrastructure.Persistence.Search;

internal sealed record ProviderListingStatement(string Sql, IReadOnlyList<NpgsqlParameter> Parameters);
