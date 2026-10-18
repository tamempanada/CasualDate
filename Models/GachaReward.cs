using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class GachaReward
{
    public int Id { get; set; }

    public GachaRewardType RewardType { get; set; } = GachaRewardType.Doll;

    public GachaRarity Rarity { get; set; } = GachaRarity.Common;

    public int? MediaId { get; set; }

    [MaxLength(80)]
    public string? BuiltInCosmeticId { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(500)]
    public string Description { get; set; } = "";

    [MaxLength(160)]
    public string LetterTitle { get; set; } = "";

    public string LetterContent { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public UploadedMedia? Media { get; set; }
}
