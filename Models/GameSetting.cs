using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class GameSetting
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string GameTitle { get; set; } = "Casual Date";

    [MaxLength(80)]
    public string GirlfriendName { get; set; } = "Ali";

    [MaxLength(80)]
    public string PlayerName { get; set; } = "";

    public string NicknamesJson { get; set; } = "[]";

    [MaxLength(500)]
    public string IntroMessage { get; set; } = "Would you like to go on a date with me, {name}?";

    [MaxLength(500)]
    public string IntroSubtitle { get; set; } = "A tiny date night made of photos, cake, questions, and favorite memories.";

    [MaxLength(500)]
    public string MainMenuMessage { get; set; } = "Welcome to our little date.";

    [MaxLength(160)]
    public string PhotoboothCompletionHeading { get; set; } = "Game Completed!";

    [MaxLength(500)]
    public string PhotoboothCompletionSubheading { get; set; } = "Your smile is as beautiful as always.";

    [MaxLength(40)]
    public string YesButtonText { get; set; } = "YES";

    [MaxLength(40)]
    public string NoButtonText { get; set; } = "NO";

    public int NoButtonDisableAfterClicks { get; set; } = 8;

    public decimal YesButtonGrowthRate { get; set; } = 0.28m;

    [MaxLength(160)]
    public string FinalTitle { get; set; } = "You completed our date";

    public string FinalMessage { get; set; } = "";

    [MaxLength(20)]
    public string PrimaryColor { get; set; } = "#d84f6f";

    [MaxLength(20)]
    public string SecondaryColor { get; set; } = "#7bb8a5";

    public int? BackgroundMediaId { get; set; }

    public int? MusicMediaId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
