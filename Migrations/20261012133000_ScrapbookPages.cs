using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261012133000_ScrapbookPages")]
    public partial class ScrapbookPages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `ScrapbookPages` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PlayerProfileId` int NOT NULL,
                    `PageNumber` int NOT NULL,
                    `Title` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `DateLabel` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                    `Body` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `PageDesign` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
                    `TextColor` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                    `TextAlign` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                    `FontSize` int NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_ScrapbookPages_PlayerProfileId_PageNumber` (`PlayerProfileId`, `PageNumber`),
                    CONSTRAINT `FK_ScrapbookPages_PlayerProfiles_PlayerProfileId`
                        FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ScrapbookPages");
        }
    }
}
