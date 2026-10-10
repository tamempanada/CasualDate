using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class ApplySettingsToPlayerPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainMenuMessage",
                table: "GameSettings",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "Welcome to our little date.")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("""
                UPDATE GameSettings
                SET MainMenuMessage = 'Welcome to our little date.',
                    CreatedAt = '2026-10-05 00:00:00',
                    UpdatedAt = '2026-10-05 00:00:00'
                WHERE Id = 1;
                """);

            migrationBuilder.Sql("""
                UPDATE MiniGames
                SET CreatedAt = '2026-10-05 00:00:00',
                    UpdatedAt = '2026-10-05 00:00:00'
                WHERE Id IN (1, 2, 3, 4);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainMenuMessage",
                table: "GameSettings");
        }
    }
}
