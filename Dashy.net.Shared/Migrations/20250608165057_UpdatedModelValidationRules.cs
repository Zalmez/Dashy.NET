using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashy.Net.Shared.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModelValidationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Sections_DashboardSectionId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_DashboardSectionId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DashboardSectionId",
                table: "Items");

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Items_SectionId",
                table: "Items",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Sections_SectionId",
                table: "Items",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Sections_SectionId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_SectionId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Items");

            migrationBuilder.AddColumn<int>(
                name: "DashboardSectionId",
                table: "Items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_DashboardSectionId",
                table: "Items",
                column: "DashboardSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Sections_DashboardSectionId",
                table: "Items",
                column: "DashboardSectionId",
                principalTable: "Sections",
                principalColumn: "Id");
        }
    }
}
