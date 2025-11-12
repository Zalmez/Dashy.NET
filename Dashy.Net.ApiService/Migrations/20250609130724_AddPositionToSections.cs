using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashy.Net.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionToSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Sections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Items");
        }
    }
}
