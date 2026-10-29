using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCityPopulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "population",
                table: "cities",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "population",
                table: "cities");
        }
    }
}
