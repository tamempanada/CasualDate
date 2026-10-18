using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261015100000_GameCompletionAttempts")]
    public partial class GameCompletionAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `GameCompletionAttempts` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `GameSlug` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
                    `AttemptToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `Status` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
                    `Score` int NULL,
                    `ResultRangeId` int NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `CompletedAt` datetime(6) NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_GameCompletionAttempts_AttemptToken` (`AttemptToken`),
                    KEY `IX_GameCompletionAttempts_PlayerProfileId_GameSlug_Status` (`PlayerProfileId`, `GameSlug`, `Status`),
                    CONSTRAINT `FK_GameCompletionAttempts_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GameCompletionAttempts");
        }
    }
}
