using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashy.Net.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AuthProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuthenticationProviders_ProviderType_IsDefault",
                table: "AuthenticationProviders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_ProviderType_IsDefault",
                table: "AuthenticationProviders",
                columns: new[] { "ProviderType", "IsDefault" });
        }
    }
}
