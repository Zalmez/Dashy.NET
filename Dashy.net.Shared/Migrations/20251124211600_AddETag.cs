using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashy.Net.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddETag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedUtc",
                table: "Sections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedUtc",
                table: "Items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedUtc",
                table: "Dashboards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModifiedUtc",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "LastModifiedUtc",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "LastModifiedUtc",
                table: "Dashboards");
        }
    }
}
