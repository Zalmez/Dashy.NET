using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashy3.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddOidcSub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OidcSub",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OidcSub",
                table: "AspNetUsers");
        }
    }
}
