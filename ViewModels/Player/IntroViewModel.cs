namespace CasualDate.ViewModels.Player;

public class IntroViewModel
{
    public string GameTitle { get; set; } = "Casual Date";

    public string GirlfriendName { get; set; } = "Ali";

    public string IntroMessage { get; set; } = "Would you like to go on a date with me?";

    public string IntroSubtitle { get; set; } = "";

    public string YesButtonText { get; set; } = "YES";

    public string NoButtonText { get; set; } = "NO";

    public int NoButtonDisableAfterClicks { get; set; } = 8;

    public decimal YesButtonGrowthRate { get; set; } = 0.28m;

    public string PrimaryColor { get; set; } = "#d84f6f";

    public string SecondaryColor { get; set; } = "#7bb8a5";
}
