using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashy3.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardAutoScroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoScroll",
                table: "Dashboards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ScrollSpeed",
                table: "Dashboards",
                type: "text",
                nullable: false,
                defaultValue: "medium");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoScroll",
                table: "Dashboards");

            migrationBuilder.DropColumn(
                name: "ScrollSpeed",
                table: "Dashboards");
        }
    }
}
