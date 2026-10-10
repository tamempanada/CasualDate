using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class PlayerIntroSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntroSubtitle",
                table: "GameSettings",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "NoButtonDisableAfterClicks",
                table: "GameSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NoButtonText",
                table: "GameSettings",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "YesButtonGrowthRate",
                table: "GameSettings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "YesButtonText",
                table: "GameSettings",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "GameSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IntroSubtitle", "NoButtonDisableAfterClicks", "NoButtonText", "UpdatedAt", "YesButtonGrowthRate", "YesButtonText" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 20, 17, 314, DateTimeKind.Utc).AddTicks(952), "A tiny date night made of photos, cake, questions, and favorite memories.", 8, "NO", new DateTime(2026, 10, 5, 16, 20, 17, 314, DateTimeKind.Utc).AddTicks(953), 0.28m, "YES" });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 20, 17, 312, DateTimeKind.Utc).AddTicks(7722), new DateTime(2026, 10, 5, 16, 20, 17, 312, DateTimeKind.Utc).AddTicks(7726) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 20, 17, 313, DateTimeKind.Utc).AddTicks(968), new DateTime(2026, 10, 5, 16, 20, 17, 313, DateTimeKind.Utc).AddTicks(969) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 20, 17, 313, DateTimeKind.Utc).AddTicks(972), new DateTime(2026, 10, 5, 16, 20, 17, 313, DateTimeKind.Utc).AddTicks(973) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 20, 17, 313, DateTimeKind.Utc).AddTicks(974), new DateTime(2026, 10, 5, 16, 20, 17, 313, DateTimeKind.Utc).AddTicks(974) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntroSubtitle",
                table: "GameSettings");

            migrationBuilder.DropColumn(
                name: "NoButtonDisableAfterClicks",
                table: "GameSettings");

            migrationBuilder.DropColumn(
                name: "NoButtonText",
                table: "GameSettings");

            migrationBuilder.DropColumn(
                name: "YesButtonGrowthRate",
                table: "GameSettings");

            migrationBuilder.DropColumn(
                name: "YesButtonText",
                table: "GameSettings");

            migrationBuilder.UpdateData(
                table: "GameSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(9083), new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(9084) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 37, 34, 168, DateTimeKind.Utc).AddTicks(6246), new DateTime(2026, 10, 5, 15, 37, 34, 168, DateTimeKind.Utc).AddTicks(6250) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(330), new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(332) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(335), new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(335) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(336), new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(337) });
        }
    }
}
