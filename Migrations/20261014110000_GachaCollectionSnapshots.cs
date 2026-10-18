using CasualDate.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261014110000_GachaCollectionSnapshots")]
    public partial class GachaCollectionSnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` ADD COLUMN IF NOT EXISTS `SnapshotName` varchar(120) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` ADD COLUMN IF NOT EXISTS `SnapshotDescription` varchar(500) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` ADD COLUMN IF NOT EXISTS `SnapshotRewardType` varchar(40) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` ADD COLUMN IF NOT EXISTS `SnapshotRarity` varchar(40) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` ADD COLUMN IF NOT EXISTS `SnapshotImagePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` ADD COLUMN IF NOT EXISTS `SnapshotLetterTitle` varchar(160) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` ADD COLUMN IF NOT EXISTS `SnapshotLetterContent` longtext CHARACTER SET utf8mb4 NULL;");

            migrationBuilder.Sql("""
                UPDATE `GachaCollectionItems` AS item
                INNER JOIN `GachaRewards` AS reward ON reward.`Id` = item.`GachaRewardId`
                LEFT JOIN `UploadedMedia` AS media ON media.`Id` = reward.`MediaId`
                SET
                    item.`SnapshotName` = CASE WHEN item.`SnapshotName` = '' THEN reward.`Name` ELSE item.`SnapshotName` END,
                    item.`SnapshotDescription` = CASE WHEN item.`SnapshotDescription` = '' THEN reward.`Description` ELSE item.`SnapshotDescription` END,
                    item.`SnapshotRewardType` = CASE
                        WHEN item.`SnapshotRewardType` <> '' THEN item.`SnapshotRewardType`
                        WHEN reward.`RewardType` = 1 THEN 'Doll'
                        WHEN reward.`RewardType` = 2 THEN 'Letter'
                        WHEN reward.`RewardType` = 3 THEN 'Poster'
                        ELSE CAST(reward.`RewardType` AS CHAR)
                    END,
                    item.`SnapshotRarity` = CASE
                        WHEN item.`SnapshotRarity` <> '' THEN item.`SnapshotRarity`
                        WHEN reward.`Rarity` = 1 THEN 'Common'
                        WHEN reward.`Rarity` = 2 THEN 'Rare'
                        WHEN reward.`Rarity` = 3 THEN 'Epic'
                        WHEN reward.`Rarity` = 4 THEN 'Legendary'
                        ELSE CAST(reward.`Rarity` AS CHAR)
                    END,
                    item.`SnapshotImagePath` = CASE WHEN item.`SnapshotImagePath` = '' THEN COALESCE(media.`FilePath`, '') ELSE item.`SnapshotImagePath` END,
                    item.`SnapshotLetterTitle` = CASE WHEN item.`SnapshotLetterTitle` = '' THEN reward.`LetterTitle` ELSE item.`SnapshotLetterTitle` END,
                    item.`SnapshotLetterContent` = CASE WHEN item.`SnapshotLetterContent` = '' OR item.`SnapshotLetterContent` IS NULL THEN reward.`LetterContent` ELSE item.`SnapshotLetterContent` END
                WHERE item.`SnapshotName` = ''
                    OR item.`SnapshotRewardType` = ''
                    OR item.`SnapshotRarity` = '';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` DROP COLUMN IF EXISTS `SnapshotLetterContent`;");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` DROP COLUMN IF EXISTS `SnapshotLetterTitle`;");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` DROP COLUMN IF EXISTS `SnapshotImagePath`;");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` DROP COLUMN IF EXISTS `SnapshotRarity`;");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` DROP COLUMN IF EXISTS `SnapshotRewardType`;");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` DROP COLUMN IF EXISTS `SnapshotDescription`;");
            migrationBuilder.Sql("ALTER TABLE `GachaCollectionItems` DROP COLUMN IF EXISTS `SnapshotName`;");
        }
    }
}
