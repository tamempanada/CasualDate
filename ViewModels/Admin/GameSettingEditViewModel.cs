using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class GameSettingEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string GameTitle { get; set; } = "";

    [Required]
    [MaxLength(80)]
    public string GirlfriendName { get; set; } = "";

    [MaxLength(80)]
    public string PlayerName { get; set; } = "";

    [MaxLength(500)]
    public string IntroMessage { get; set; } = "";

    [MaxLength(500)]
    public string IntroSubtitle { get; set; } = "";

    [MaxLength(500)]
    public string MainMenuMessage { get; set; } = "";

    [MaxLength(160)]
    public string PhotoboothCompletionHeading { get; set; } = "";

    [MaxLength(500)]
    public string PhotoboothCompletionSubheading { get; set; } = "";

    [Required]
    [MaxLength(40)]
    public string YesButtonText { get; set; } = "YES";

    [Required]
    [MaxLength(40)]
    public string NoButtonText { get; set; } = "NO";

    [Display(Name = "No click limit")]
    [Range(1, 20)]
    public int NoButtonDisableAfterClicks { get; set; } = 8;

    [Range(0.05, 1.00)]
    public decimal YesButtonGrowthRate { get; set; } = 0.28m;

    [Required]
    [MaxLength(160)]
    public string FinalTitle { get; set; } = "";

    public string FinalMessage { get; set; } = "";

    [Required]
    [MaxLength(20)]
    public string PrimaryColor { get; set; } = "#d84f6f";

    [Required]
    [MaxLength(20)]
    public string SecondaryColor { get; set; } = "#7bb8a5";

    public int? BackgroundMediaId { get; set; }

    public int? MusicMediaId { get; set; }
}
