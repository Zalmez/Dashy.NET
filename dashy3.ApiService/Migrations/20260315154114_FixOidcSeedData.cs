using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashy3.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class FixOidcSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "OidcConfig",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "OidcConfig",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 3, 15, 15, 32, 7, 778, DateTimeKind.Utc).AddTicks(9171));
        }
    }
}
