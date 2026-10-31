using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vitrio.Infrastructure.Persistence.Search;

namespace Vitrio.Infrastructure.Persistence;

public static class AppDbContextOptions
{
    public const string ConnectionStringName = "Database";

    public static DbContextOptionsBuilder UseVitrioDatabase(this DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseNpgsql(WithSearchSettings(connectionString), npgsql => npgsql
                .UseNetTopologySuite(geographyAsDefault: true)
                .MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention();

    private static string WithSearchSettings(string connectionString)
    {
        // O operador <% da busca por texto só usa o índice trigram com o limiar vindo desta configuração da sessão.
        // O JIT compila a busca por mais de um segundo quando o custo estimado é alto, para consultas que rodam em milissegundos.
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        builder.Options = $"{builder.Options} -c pg_trgm.word_similarity_threshold={ProviderSearchIndex.MinWordSimilarity} -c jit=off".Trim();
        return builder.ConnectionString;
    }
}
