using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using NpgsqlTypes;

#nullable disable

namespace Vitrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "search_name",
                table: "providers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "search_primary",
                table: "providers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "search_secondary",
                table: "providers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "search_terms",
                table: "providers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "public_location",
                table: "providers",
                type: "geography (point, 4326)",
                nullable: true,
                computedColumnSql: "st_setsrid(st_makepoint(public_longitude, public_latitude), 4326)::geography",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "providers",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "setweight(to_tsvector('portuguese'::regconfig, coalesce(search_primary, '')), 'A') || setweight(to_tsvector('portuguese'::regconfig, coalesce(search_name, '')), 'B') || setweight(to_tsvector('portuguese'::regconfig, coalesce(search_secondary, '')), 'C')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_providers_public_location",
                table: "providers",
                column: "public_location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_providers_search_terms",
                table: "providers",
                column: "search_terms")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_providers_search_vector",
                table: "providers",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.Sql("""
                update providers p set
                    search_primary = lower(unaccent(concat_ws(' ',
                        p.headline,
                        (select string_agg(c.name, ' ' order by pc.is_primary desc)
                         from provider_categories pc join categories c on c.id = pc.category_id
                         where pc.provider_id = p.id),
                        (select string_agg(s.title, ' ' order by s.sort_order) from provider_services s where s.provider_id = p.id)))),
                    search_name = lower(unaccent(p.display_name)),
                    search_secondary = lower(unaccent(concat_ws(' ',
                        (select string_agg(s.description, ' ' order by s.sort_order) from provider_services s where s.provider_id = p.id),
                        p.about)));
                update providers set search_terms = concat_ws(' ', search_primary, search_name);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_providers_public_location",
                table: "providers");

            migrationBuilder.DropIndex(
                name: "ix_providers_search_terms",
                table: "providers");

            migrationBuilder.DropIndex(
                name: "ix_providers_search_vector",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "public_location",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "search_vector",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "search_name",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "search_primary",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "search_secondary",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "search_terms",
                table: "providers");
        }
    }
}
