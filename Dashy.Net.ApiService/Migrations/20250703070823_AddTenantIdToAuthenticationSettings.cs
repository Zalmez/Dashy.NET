using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashy.Net.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToAuthenticationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AuthenticationSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuthenticationSettings");
        }
    }
}
