using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderRankingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "search_completeness",
                table: "providers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "search_service_price_cents",
                table: "providers",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                update providers p set
                    search_completeness = (
                        (p.avatar_key is not null)::int
                        + ((select count(*) from portfolio_photos photo where photo.provider_id = p.id and photo.status = 'Active') >= 3)::int
                        + (exists (select 1 from availability_slots slot where slot.provider_id = p.id))::int
                        + (p.years_of_experience is not null)::int) / 4.0,
                    search_service_price_cents = (
                        select min(service.price_from_cents) from provider_services service
                        where service.provider_id = p.id and not service.on_quote and service.price_unit = 'PerService');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "search_completeness",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "search_service_price_cents",
                table: "providers");
        }
    }
}
