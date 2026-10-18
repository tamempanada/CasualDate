using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class LoveBlocksGameAndRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @column_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TokenRewardSettings' AND COLUMN_NAME = 'TriggerInterval'
                );
                SET @column_sql = IF(@column_exists = 0,
                    'ALTER TABLE `TokenRewardSettings` ADD COLUMN `TriggerInterval` int NOT NULL DEFAULT 1',
                    'SELECT 1');
                PREPARE love_blocks_column_stmt FROM @column_sql;
                EXECUTE love_blocks_column_stmt;
                DEALLOCATE PREPARE love_blocks_column_stmt;
                """);
            migrationBuilder.Sql("UPDATE `TokenRewardSettings` SET `TriggerInterval` = 1 WHERE `TriggerInterval` = 0;");
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `LoveBlocksPlayerRecords` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `BestScore` int NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_LoveBlocksPlayerRecords_PlayerProfileId` (`PlayerProfileId`),
                    CONSTRAINT `FK_LoveBlocksPlayerRecords_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `LoveBlocksRuns` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `RunToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `LastRequestId` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `Board` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
                    `PiecesJson` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `ProcessedRequestIdsJson` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `Score` int NOT NULL,
                    `LinesCleared` int NOT NULL,
                    `MovesMade` int NOT NULL,
                    `TokensEarned` int NOT NULL,
                    `ScoreMilestonesAwarded` int NOT NULL,
                    `LineMilestonesAwarded` int NOT NULL,
                    `PersonalBestRewarded` tinyint(1) NOT NULL,
                    `IsOver` tinyint(1) NOT NULL,
                    `ScoreRewardAmount` int NOT NULL,
                    `ScoreRewardInterval` int NOT NULL,
                    `ScoreRewardEnabled` tinyint(1) NOT NULL,
                    `LineRewardAmount` int NOT NULL,
                    `LineRewardInterval` int NOT NULL,
                    `LineRewardEnabled` tinyint(1) NOT NULL,
                    `BestRewardAmount` int NOT NULL,
                    `BestRewardEnabled` tinyint(1) NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_LoveBlocksRuns_RunToken` (`RunToken`),
                    KEY `IX_LoveBlocksRuns_PlayerProfileId_IsOver` (`PlayerProfileId`, `IsOver`),
                    CONSTRAINT `FK_LoveBlocksRuns_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "IconLabel", "Name", "Slug" },
                values: new object[] { "Place the pieces, clear the board, and beat your high score.", "Blocks", "Love Blocks", "love-blocks" });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Keep Love Blocks runs, best scores, and configured intervals if this
            // feature is rolled back; removing this schema would erase player data.
            migrationBuilder.UpdateData(
                table: "MiniGames",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "IconLabel", "Name", "Slug" },
                values: new object[] { "Write and save personal diary pages.", "Diary", "Scrapbook Diary", "scrapbook" });
        }
    }
}
