using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261012124500_GachaRewardsAndCollection")]
    public partial class GachaRewardsAndCollection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `GachaRewards` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `RewardType` int NOT NULL,
                    `Rarity` int NOT NULL,
                    `MediaId` int NULL,
                    `Name` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
                    `LetterTitle` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
                    `LetterContent` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `IsActive` tinyint(1) NOT NULL,
                    `DisplayOrder` int NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    KEY `IX_GachaRewards_MediaId` (`MediaId`),
                    CONSTRAINT `FK_GachaRewards_UploadedMedia_MediaId`
                        FOREIGN KEY (`MediaId`) REFERENCES `UploadedMedia` (`Id`) ON DELETE SET NULL
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `GachaCollectionItems` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `GachaRewardId` int NOT NULL,
                    `Quantity` int NOT NULL,
                    `FirstKeptAt` datetime(6) NOT NULL,
                    `LastKeptAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    KEY `IX_GachaCollectionItems_GachaRewardId` (`GachaRewardId`),
                    UNIQUE KEY `IX_GachaCollectionItems_PlayerProfileId_GachaRewardId` (`PlayerProfileId`, `GachaRewardId`),
                    CONSTRAINT `FK_GachaCollectionItems_GachaRewards_GachaRewardId`
                        FOREIGN KEY (`GachaRewardId`) REFERENCES `GachaRewards` (`Id`) ON DELETE RESTRICT,
                    CONSTRAINT `FK_GachaCollectionItems_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `GachaPulls` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `GachaRewardId` int NOT NULL,
                    `Status` int NOT NULL,
                    `PullToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `ResolvedAt` datetime(6) NULL,
                    PRIMARY KEY (`Id`),
                    KEY `IX_GachaPulls_GachaRewardId` (`GachaRewardId`),
                    KEY `IX_GachaPulls_PlayerProfileId` (`PlayerProfileId`),
                    UNIQUE KEY `IX_GachaPulls_PullToken` (`PullToken`),
                    CONSTRAINT `FK_GachaPulls_GachaRewards_GachaRewardId`
                        FOREIGN KEY (`GachaRewardId`) REFERENCES `GachaRewards` (`Id`) ON DELETE RESTRICT,
                    CONSTRAINT `FK_GachaPulls_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GachaCollectionItems");
            migrationBuilder.DropTable(name: "GachaPulls");
            migrationBuilder.DropTable(name: "GachaRewards");
        }
    }
}
