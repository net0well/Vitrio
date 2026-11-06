using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_title = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    preferred_day = table.Column<DateOnly>(type: "date", nullable: false),
                    preferred_period = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    address_street = table.Column<string>(type: "text", nullable: true),
                    address_number = table.Column<string>(type: "text", nullable: true),
                    address_complement = table.Column<string>(type: "text", nullable: true),
                    address_cep = table.Column<string>(type: "text", nullable: true),
                    customer_full_name = table.Column<string>(type: "text", nullable: true),
                    customer_phone = table.Column<string>(type: "text", nullable: true),
                    customer_display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    city_ibge_code = table.Column<int>(type: "integer", nullable: false),
                    city_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    neighborhood = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    customer_latitude = table.Column<double>(type: "double precision", nullable: false),
                    customer_longitude = table.Column<double>(type: "double precision", nullable: false),
                    customer_point_is_approximate = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    respond_by = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    first_response_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decline_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    decline_details = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    quote_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    quote_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    quote_date = table.Column<DateOnly>(type: "date", nullable: true),
                    quote_start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    quote_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    quote_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    quote_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    quote_material_included = table.Column<bool>(type: "boolean", nullable: true),
                    quote_material_details = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    quote_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quote_sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    quote_version = table.Column<int>(type: "integer", nullable: true),
                    quote_valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    scheduled_date = table.Column<DateOnly>(type: "date", nullable: true),
                    scheduled_start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    scheduled_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    scheduled_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    scheduled_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reschedule_count = table.Column<int>(type: "integer", nullable: false),
                    address_released_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_by = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_by = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cancellation_details = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    personal_data_erased_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_users_customer_id",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    upload_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    object_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_photos_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_photos_order_id",
                table: "order_photos",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_photos_processing",
                table: "order_photos",
                column: "created_at",
                filter: "status = 'Processing'");

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id_created_at",
                table: "orders",
                columns: new[] { "customer_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_orders_provider_id_status",
                table: "orders",
                columns: new[] { "provider_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_scheduled_start",
                table: "orders",
                column: "scheduled_start");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status_quote_valid_until",
                table: "orders",
                columns: new[] { "status", "quote_valid_until" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_status_respond_by",
                table: "orders",
                columns: new[] { "status", "respond_by" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_photos");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
