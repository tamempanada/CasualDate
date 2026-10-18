namespace CasualDate.Models;

using System.ComponentModel.DataAnnotations;

public class GachaCollectionItem
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    public int GachaRewardId { get; set; }

    public int Quantity { get; set; } = 1;

    [MaxLength(120)]
    public string SnapshotName { get; set; } = "";

    [MaxLength(500)]
    public string SnapshotDescription { get; set; } = "";

    [MaxLength(40)]
    public string SnapshotRewardType { get; set; } = "";

    [MaxLength(40)]
    public string SnapshotRarity { get; set; } = "";

    [MaxLength(500)]
    public string SnapshotImagePath { get; set; } = "";

    [MaxLength(160)]
    public string SnapshotLetterTitle { get; set; } = "";

    public string SnapshotLetterContent { get; set; } = "";

    public DateTime FirstKeptAt { get; set; } = DateTime.UtcNow;

    public DateTime LastKeptAt { get; set; } = DateTime.UtcNow;

    public GachaReward? Reward { get; set; }
}
