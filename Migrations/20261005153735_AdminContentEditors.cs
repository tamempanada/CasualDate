using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class AdminContentEditors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "GameSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IntroMessage", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(9083), "Would you like to go on a date with me, {name}?", new DateTime(2026, 10, 5, 15, 37, 34, 169, DateTimeKind.Utc).AddTicks(9084) });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "GameSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IntroMessage", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 20, 35, 482, DateTimeKind.Utc).AddTicks(535), "Would you like to go on a date with me?", new DateTime(2026, 10, 5, 15, 20, 35, 482, DateTimeKind.Utc).AddTicks(536) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 20, 35, 480, DateTimeKind.Utc).AddTicks(8569), new DateTime(2026, 10, 5, 15, 20, 35, 480, DateTimeKind.Utc).AddTicks(8573) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 20, 35, 481, DateTimeKind.Utc).AddTicks(1365), new DateTime(2026, 10, 5, 15, 20, 35, 481, DateTimeKind.Utc).AddTicks(1365) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 20, 35, 481, DateTimeKind.Utc).AddTicks(1367), new DateTime(2026, 10, 5, 15, 20, 35, 481, DateTimeKind.Utc).AddTicks(1368) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 15, 20, 35, 481, DateTimeKind.Utc).AddTicks(1369), new DateTime(2026, 10, 5, 15, 20, 35, 481, DateTimeKind.Utc).AddTicks(1369) });
        }
    }
}
