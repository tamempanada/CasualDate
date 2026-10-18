using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261013103000_BlackjackRounds")]
    public partial class BlackjackRounds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `BlackjackRounds` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `RoundToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `BetAmount` int NOT NULL,
                    `Status` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
                    `PayoutAmount` int NOT NULL,
                    `DeckJson` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `PlayerHandJson` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `DealerHandJson` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    `ResolvedAt` datetime(6) NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_BlackjackRounds_RoundToken` (`RoundToken`),
                    KEY `IX_BlackjackRounds_PlayerProfileId_Status` (`PlayerProfileId`, `Status`),
                    CONSTRAINT `FK_BlackjackRounds_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BlackjackRounds");
        }
    }
}
