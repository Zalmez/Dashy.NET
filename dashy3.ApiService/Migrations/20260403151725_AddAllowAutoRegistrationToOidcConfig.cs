using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dashy3.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowAutoRegistrationToOidcConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowAutoRegistration",
                table: "OidcConfig",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmailConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SmtpHost = table.Column<string>(type: "text", nullable: false),
                    SmtpPort = table.Column<int>(type: "integer", nullable: false),
                    SmtpUseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    SmtpUsername = table.Column<string>(type: "text", nullable: false),
                    SmtpPassword = table.Column<string>(type: "text", nullable: false),
                    SmtpFromEmail = table.Column<string>(type: "text", nullable: false),
                    SmtpFromName = table.Column<string>(type: "text", nullable: false),
                    GraphTenantId = table.Column<string>(type: "text", nullable: false),
                    GraphClientId = table.Column<string>(type: "text", nullable: false),
                    GraphClientSecret = table.Column<string>(type: "text", nullable: false),
                    GraphFromEmail = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invites",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    InvitedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcceptedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invites", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EmailConfig",
                columns: new[] { "Id", "GraphClientId", "GraphClientSecret", "GraphFromEmail", "GraphTenantId", "IsEnabled", "Provider", "SmtpFromEmail", "SmtpFromName", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUseSsl", "SmtpUsername", "UpdatedAt" },
                values: new object[] { 1, "", "", "", "", false, "None", "", "Dashy", "", "", 587, true, "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "OidcConfig",
                keyColumn: "Id",
                keyValue: 1,
                column: "AllowAutoRegistration",
                value: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invites_Email",
                table: "Invites",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_Token",
                table: "Invites",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailConfig");

            migrationBuilder.DropTable(
                name: "Invites");

            migrationBuilder.DropColumn(
                name: "AllowAutoRegistration",
                table: "OidcConfig");
        }
    }
}
