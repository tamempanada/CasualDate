using System.ComponentModel.DataAnnotations;
using CasualDate.Models;

namespace CasualDate.ViewModels.Admin;

public class GachaRewardEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Reward type")]
    public GachaRewardType RewardType { get; set; } = GachaRewardType.Doll;

    public GachaRarity Rarity { get; set; } = GachaRarity.Common;

    [Display(Name = "Artwork")]
    public int? MediaId { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = "";

    [StringLength(500)]
    public string Description { get; set; } = "";

    [Display(Name = "Letter title")]
    [StringLength(160)]
    public string LetterTitle { get; set; } = "";

    [Display(Name = "Letter text")]
    public string LetterContent { get; set; } = "";

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }
}
