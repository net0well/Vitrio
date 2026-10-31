using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryProfessionalNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "professional_plural",
                table: "categories",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "professional_singular",
                table: "categories",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "eletricistas", "eletricista" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "encanadores", "encanador" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "pedreiros", "pedreiro" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "pintores", "pintor" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "diaristas", "diarista" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "jardineiros", "jardineiro" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "montadores de móveis", "montador de móveis" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "técnicos de ar-condicionado", "técnico de ar-condicionado" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "maridos de aluguel", "marido de aluguel" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "freteiros", "freteiro" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 11,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "profissionais de beleza", "profissional de beleza" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 12,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "professores particulares", "professor particular" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 13,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "técnicos de informática", "técnico de informática" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 14,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "cuidadores", "cuidador" });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 15,
                columns: new[] { "professional_plural", "professional_singular" },
                values: new object[] { "profissionais para pet", "profissional para pet" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "professional_plural",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "professional_singular",
                table: "categories");
        }
    }
}
