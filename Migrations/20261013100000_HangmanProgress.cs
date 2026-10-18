using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261013100000_HangmanProgress")]
    public partial class HangmanProgress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `HangmanProgresses` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `AdventureUnlockedLevel` int NOT NULL,
                    `AdventureCompletedLevels` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
                    `EndlessTotalWins` int NOT NULL,
                    `EndlessBestStreak` int NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_HangmanProgresses_PlayerProfileId` (`PlayerProfileId`),
                    CONSTRAINT `FK_HangmanProgresses_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HangmanProgresses");
        }
    }
}
