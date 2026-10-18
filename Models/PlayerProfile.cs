using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class PlayerProfile
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string ApplicationUserId { get; set; } = "";

    [MaxLength(120)]
    public string DisplayName { get; set; } = "Player";

    public int TokenBalance { get; set; }

    [MaxLength(80)]
    public string EquippedThemeId { get; set; } = BuiltInCosmeticCatalog.DefaultThemeId;

    [MaxLength(80)]
    public string EquippedMusicSkinId { get; set; } = BuiltInCosmeticCatalog.DefaultMusicSkinId;

    [MaxLength(80)]
    public string EquippedEffectId { get; set; } = BuiltInCosmeticCatalog.NoEffectId;

    public int? EquippedCompanionRewardId { get; set; }

    public int? SelectedMusicMediaId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
