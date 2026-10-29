using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Vitrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    slug = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    icon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    headline = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    about = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    years_of_experience = table.Column<int>(type: "integer", nullable: true),
                    avatar_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contact_phone = table.Column<string>(type: "text", nullable: true),
                    whats_app_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    city_ibge_code = table.Column<int>(type: "integer", nullable: true),
                    base_cep = table.Column<string>(type: "text", nullable: true),
                    base_street = table.Column<string>(type: "text", nullable: true),
                    base_number = table.Column<string>(type: "text", nullable: true),
                    base_complement = table.Column<string>(type: "text", nullable: true),
                    base_neighborhood = table.Column<string>(type: "text", nullable: true),
                    base_location = table.Column<string>(type: "text", nullable: true),
                    public_latitude = table.Column<double>(type: "double precision", nullable: true),
                    public_longitude = table.Column<double>(type: "double precision", nullable: true),
                    location_approximate = table.Column<bool>(type: "boolean", nullable: false),
                    service_radius_meters = table.Column<int>(type: "integer", nullable: false),
                    available_today = table.Column<bool>(type: "boolean", nullable: false),
                    available_today_set_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    handles_emergencies = table.Column<bool>(type: "boolean", nullable: false),
                    rating_score = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    review_count = table.Column<int>(type: "integer", nullable: false),
                    completed_orders = table.Column<int>(type: "integer", nullable: false),
                    response_time_bucket = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    response_sample_size = table.Column<int>(type: "integer", nullable: false),
                    late_cancellations90d = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_providers", x => x.id);
                    table.ForeignKey(
                        name: "fk_providers_cities_city_ibge_code",
                        column: x => x.city_ibge_code,
                        principalTable: "cities",
                        principalColumn: "ibge_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_providers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "availability_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_minute = table.Column<int>(type: "integer", nullable: false),
                    end_minute = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_availability_slots", x => x.id);
                    table.ForeignKey(
                        name: "fk_availability_slots_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_provider_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_provider_categories_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: true),
                    price_from_cents = table.Column<long>(type: "bigint", nullable: true),
                    price_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    on_quote = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_provider_services_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "served_neighborhoods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    name_normalized = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_served_neighborhoods", x => x.id);
                    table.ForeignKey(
                        name: "fk_served_neighborhoods_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "active", "icon", "name", "slug", "sort_order" },
                values: new object[,]
                {
                    { 1, true, "bolt", "Elétrica", "eletrica", 1 },
                    { 2, true, "droplet", "Hidráulica", "hidraulica", 2 },
                    { 3, true, "brick", "Pedreiro", "pedreiro", 3 },
                    { 4, true, "roller", "Pintura", "pintura", 4 },
                    { 5, true, "broom", "Diarista", "diarista", 5 },
                    { 6, true, "leaf", "Jardinagem", "jardinagem", 6 },
                    { 7, true, "wrench", "Montagem de móveis", "montagem-de-moveis", 7 },
                    { 8, true, "sliders", "Ar-condicionado", "ar-condicionado", 8 },
                    { 9, true, "wrench", "Marido de aluguel", "marido-de-aluguel", 9 },
                    { 10, true, "transfer", "Fretes e mudanças", "fretes-e-mudancas", 10 },
                    { 11, true, "star", "Beleza a domicílio", "beleza-a-domicilio", 11 },
                    { 12, true, "message", "Aulas particulares", "aulas-particulares", 12 },
                    { 13, true, "grid", "Informática", "informatica", 13 },
                    { 14, true, "user", "Cuidador(a)", "cuidador", 14 },
                    { 15, true, "heart", "Pet", "pet", 15 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_availability_slots_provider_id",
                table: "availability_slots",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_categories_category_id",
                table: "provider_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_provider_categories_provider_id_category_id",
                table: "provider_categories",
                columns: new[] { "provider_id", "category_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_services_provider_id",
                table: "provider_services",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_providers_city_ibge_code",
                table: "providers",
                column: "city_ibge_code");

            migrationBuilder.CreateIndex(
                name: "ix_providers_user_id",
                table: "providers",
                column: "user_id",
                unique: true,
                filter: "status <> 'Removed'");

            migrationBuilder.CreateIndex(
                name: "ix_served_neighborhoods_provider_id_name_normalized",
                table: "served_neighborhoods",
                columns: new[] { "provider_id", "name_normalized" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "availability_slots");

            migrationBuilder.DropTable(
                name: "provider_categories");

            migrationBuilder.DropTable(
                name: "provider_services");

            migrationBuilder.DropTable(
                name: "served_neighborhoods");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "providers");
        }
    }
}
