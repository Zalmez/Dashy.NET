using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashy.Net.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AuthProviders3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scopes",
                table: "AuthenticationSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scopes",
                table: "AuthenticationSettings");
        }
    }
}
