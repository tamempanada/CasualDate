using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class StabilizeSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "GameSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "GameSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 32, 49, 803, DateTimeKind.Utc).AddTicks(5050), new DateTime(2026, 10, 5, 16, 32, 49, 803, DateTimeKind.Utc).AddTicks(5051) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 32, 49, 801, DateTimeKind.Utc).AddTicks(9976), new DateTime(2026, 10, 5, 16, 32, 49, 801, DateTimeKind.Utc).AddTicks(9980) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 32, 49, 802, DateTimeKind.Utc).AddTicks(3683), new DateTime(2026, 10, 5, 16, 32, 49, 802, DateTimeKind.Utc).AddTicks(3684) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 32, 49, 802, DateTimeKind.Utc).AddTicks(3687), new DateTime(2026, 10, 5, 16, 32, 49, 802, DateTimeKind.Utc).AddTicks(3687) });

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 32, 49, 802, DateTimeKind.Utc).AddTicks(3688), new DateTime(2026, 10, 5, 16, 32, 49, 802, DateTimeKind.Utc).AddTicks(3689) });
        }
    }
}
