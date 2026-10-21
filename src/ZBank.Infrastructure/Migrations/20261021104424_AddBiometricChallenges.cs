using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZBank.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBiometricChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BiometricChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nonce = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricChallenges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BiometricChallenges_DeviceId_Nonce",
                table: "BiometricChallenges",
                columns: new[] { "DeviceId", "Nonce" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BiometricChallenges");
        }
    }
}
