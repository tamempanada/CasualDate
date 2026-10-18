using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261014100000_GachaEconomy")]
    public partial class GachaEconomy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `GachaPullBatches` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `RequestToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `BatchToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `PullCount` int NOT NULL,
                    `TokenCost` int NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_GachaPullBatches_BatchToken` (`BatchToken`),
                    UNIQUE KEY `IX_GachaPullBatches_PlayerProfileId_RequestToken` (`PlayerProfileId`, `RequestToken`),
                    CONSTRAINT `FK_GachaPullBatches_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `GachaPityStates` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `LegendaryPityCount` int NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_GachaPityStates_PlayerProfileId` (`PlayerProfileId`),
                    CONSTRAINT `FK_GachaPityStates_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);

            // Runtime schema repair in Program.cs adds the GachaPulls extension columns.
            // This keeps the migration safe for existing databases where those columns were
            // already created during an earlier repair after a failed application startup.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GachaPityStates");
            migrationBuilder.DropTable(name: "GachaPullBatches");
        }
    }
}
