using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class PlayerWalletFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @heading_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                        AND TABLE_NAME = 'GameSettings'
                        AND COLUMN_NAME = 'PhotoboothCompletionHeading'
                );
                SET @heading_sql = IF(
                    @heading_exists = 0,
                    'ALTER TABLE `GameSettings` ADD `PhotoboothCompletionHeading` varchar(160) CHARACTER SET utf8mb4 NOT NULL DEFAULT ''Game Completed!'';',
                    'SELECT 1;'
                );
                PREPARE heading_stmt FROM @heading_sql;
                EXECUTE heading_stmt;
                DEALLOCATE PREPARE heading_stmt;
                """);

            migrationBuilder.Sql("""
                SET @subheading_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                        AND TABLE_NAME = 'GameSettings'
                        AND COLUMN_NAME = 'PhotoboothCompletionSubheading'
                );
                SET @subheading_sql = IF(
                    @subheading_exists = 0,
                    'ALTER TABLE `GameSettings` ADD `PhotoboothCompletionSubheading` varchar(500) CHARACTER SET utf8mb4 NOT NULL DEFAULT ''Your smile is as beautiful as always.'';',
                    'SELECT 1;'
                );
                PREPARE subheading_stmt FROM @subheading_sql;
                EXECUTE subheading_stmt;
                DEALLOCATE PREPARE subheading_stmt;
                """);

            migrationBuilder.Sql("""
                UPDATE GameSettings
                SET PhotoboothCompletionHeading = 'Game Completed!',
                    PhotoboothCompletionSubheading = 'Your smile is as beautiful as always.'
                WHERE Id = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @heading_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                        AND TABLE_NAME = 'GameSettings'
                        AND COLUMN_NAME = 'PhotoboothCompletionHeading'
                );
                SET @heading_sql = IF(
                    @heading_exists = 1,
                    'ALTER TABLE `GameSettings` DROP COLUMN `PhotoboothCompletionHeading`;',
                    'SELECT 1;'
                );
                PREPARE heading_stmt FROM @heading_sql;
                EXECUTE heading_stmt;
                DEALLOCATE PREPARE heading_stmt;
                """);

            migrationBuilder.Sql("""
                SET @subheading_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                        AND TABLE_NAME = 'GameSettings'
                        AND COLUMN_NAME = 'PhotoboothCompletionSubheading'
                );
                SET @subheading_sql = IF(
                    @subheading_exists = 1,
                    'ALTER TABLE `GameSettings` DROP COLUMN `PhotoboothCompletionSubheading`;',
                    'SELECT 1;'
                );
                PREPARE subheading_stmt FROM @subheading_sql;
                EXECUTE subheading_stmt;
                DEALLOCATE PREPARE subheading_stmt;
                """);
        }
    }
}
