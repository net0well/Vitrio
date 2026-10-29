using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_upload_key",
                table: "providers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "portfolio_photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    upload_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    object_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    thumbnail_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    caption = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_portfolio_photos_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_photos_processing",
                table: "portfolio_photos",
                column: "created_at",
                filter: "status = 'Processing'");

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_photos_provider_id_sort_order",
                table: "portfolio_photos",
                columns: new[] { "provider_id", "sort_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portfolio_photos");

            migrationBuilder.DropColumn(
                name: "avatar_upload_key",
                table: "providers");
        }
    }
}
