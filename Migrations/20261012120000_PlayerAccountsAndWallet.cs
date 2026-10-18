using System;
using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261012120000_PlayerAccountsAndWallet")]
    public partial class PlayerAccountsAndWallet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `PlayerProfiles` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `ApplicationUserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `DisplayName` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `TokenBalance` int NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_PlayerProfiles_ApplicationUserId` (`ApplicationUserId`),
                    CONSTRAINT `FK_PlayerProfiles_AspNetUsers_ApplicationUserId`
                        FOREIGN KEY (`ApplicationUserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `TokenRewardSettings` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `EventKey` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `DisplayName` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
                    `TokenAmount` int NOT NULL,
                    `IsEnabled` tinyint(1) NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_TokenRewardSettings_EventKey` (`EventKey`)
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `PlayerGameCompletions` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `EventKey` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `TokenAmountAwarded` int NOT NULL,
                    `CompletedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_PlayerGameCompletions_PlayerProfileId_EventKey` (`PlayerProfileId`, `EventKey`),
                    CONSTRAINT `FK_PlayerGameCompletions_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `TokenTransactions` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `Amount` int NOT NULL,
                    `BalanceAfter` int NOT NULL,
                    `TransactionType` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `ReferenceKey` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `Description` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    KEY `IX_TokenTransactions_PlayerProfileId` (`PlayerProfileId`),
                    CONSTRAINT `FK_TokenTransactions_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO `MiniGames` (`Id`, `CreatedAt`, `Description`, `DisplayOrder`, `IconLabel`, `IsEnabled`, `Name`, `Slug`, `UpdatedAt`) VALUES
                    (5, '2026-10-05 00:00:00.000000', 'Pull collectible dolls, letters, and posters.', 5, 'Gacha', TRUE, 'Girlfriend Gacha', 'gacha', '2026-10-05 00:00:00.000000'),
                    (6, '2026-10-05 00:00:00.000000', 'Write and save personal diary pages.', 6, 'Diary', TRUE, 'Scrapbook Diary', 'scrapbook', '2026-10-05 00:00:00.000000'),
                    (7, '2026-10-05 00:00:00.000000', 'Guess words across adventure and endless modes.', 7, 'Word', TRUE, 'Hangman', 'hangman', '2026-10-05 00:00:00.000000'),
                    (8, '2026-10-05 00:00:00.000000', 'Play cards using your shared token wallet.', 8, 'Cards', TRUE, 'Blackjack', 'blackjack', '2026-10-05 00:00:00.000000');
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO `TokenRewardSettings` (`Id`, `CreatedAt`, `DisplayName`, `EventKey`, `IsEnabled`, `TokenAmount`, `UpdatedAt`) VALUES
                    (1, '2026-10-05 00:00:00.000000', 'Complete Photobooth', 'complete:photobooth', TRUE, 20, '2026-10-05 00:00:00.000000'),
                    (2, '2026-10-05 00:00:00.000000', 'Complete Bake a Cake', 'complete:cake', TRUE, 25, '2026-10-05 00:00:00.000000'),
                    (3, '2026-10-05 00:00:00.000000', 'Complete Perfect Date', 'complete:perfect-date', TRUE, 35, '2026-10-05 00:00:00.000000'),
                    (4, '2026-10-05 00:00:00.000000', 'Complete Quiz', 'complete:quiz', TRUE, 30, '2026-10-05 00:00:00.000000');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlayerGameCompletions");
            migrationBuilder.DropTable(name: "TokenTransactions");
            migrationBuilder.DropTable(name: "TokenRewardSettings");
            migrationBuilder.DropTable(name: "PlayerProfiles");
        }
    }
}
