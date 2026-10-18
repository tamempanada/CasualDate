using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261016030000_AddCosmeticPreferencesV13")]
public partial class AddCosmeticPreferencesV13 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        AddColumnIfMissing(migrationBuilder, "GachaRewards", "BuiltInCosmeticId", "varchar(80) CHARACTER SET utf8mb4 NULL");
        AddColumnIfMissing(migrationBuilder, "PlayerProfiles", "EquippedThemeId", "varchar(80) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'default-theme'");
        AddColumnIfMissing(migrationBuilder, "PlayerProfiles", "EquippedMusicSkinId", "varchar(80) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'default-music'");
        AddColumnIfMissing(migrationBuilder, "PlayerProfiles", "EquippedEffectId", "varchar(80) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'none-effect'");
        AddColumnIfMissing(migrationBuilder, "PlayerProfiles", "EquippedCompanionRewardId", "int NULL");
        AddColumnIfMissing(migrationBuilder, "PlayerProfiles", "SelectedMusicMediaId", "int NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Existing player preferences are intentionally preserved on downgrade.
    }

    private static void AddColumnIfMissing(MigrationBuilder migrationBuilder, string table, string column, string definition)
    {
        migrationBuilder.Sql($"""
            SET @column_exists = (
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{table}' AND COLUMN_NAME = '{column}'
            );
            SET @column_sql = IF(@column_exists = 0,
                'ALTER TABLE `{table}` ADD COLUMN `{column}` {definition}',
                'SELECT 1');
            PREPARE column_stmt FROM @column_sql;
            EXECUTE column_stmt;
            DEALLOCATE PREPARE column_stmt;
            """);
    }
}
