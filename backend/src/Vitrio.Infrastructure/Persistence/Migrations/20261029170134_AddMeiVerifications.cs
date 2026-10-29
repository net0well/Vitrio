using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMeiVerifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mei_verifications",
                columns: table => new
                {
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cnpj = table.Column<string>(type: "character(14)", fixedLength: true, maxLength: 14, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    checked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    pending_since = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    company_name = table.Column<string>(type: "text", nullable: true),
                    reason_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mei_verifications", x => x.provider_id);
                    table.ForeignKey(
                        name: "fk_mei_verifications_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mei_verifications_cnpj",
                table: "mei_verifications",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mei_verifications_status_checked_at",
                table: "mei_verifications",
                columns: new[] { "status", "checked_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mei_verifications");
        }
    }
}
