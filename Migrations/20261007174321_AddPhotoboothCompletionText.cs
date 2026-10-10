using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoboothCompletionText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoboothCompletionHeading",
                table: "GameSettings",
                type: "varchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "Game Completed!");

            migrationBuilder.AddColumn<string>(
                name: "PhotoboothCompletionSubheading",
                table: "GameSettings",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "Your smile is as beautiful as always.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoboothCompletionHeading",
                table: "GameSettings");

            migrationBuilder.DropColumn(
                name: "PhotoboothCompletionSubheading",
                table: "GameSettings");
        }
    }
}
