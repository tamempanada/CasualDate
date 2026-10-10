using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class PlayerIntroSettings2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "GameSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 16, 20, 17, 314, DateTimeKind.Utc).AddTicks(952), new DateTime(2026, 10, 5, 16, 20, 17, 314, DateTimeKind.Utc).AddTicks(953) });

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
    }
}
