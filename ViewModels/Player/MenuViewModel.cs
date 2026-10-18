namespace CasualDate.ViewModels.Player;

public class MenuViewModel
{
    public string GameTitle { get; set; } = "Casual Date";

    public string WelcomeMessage { get; set; } = "Welcome to our little date.";

    public string PrimaryColor { get; set; } = "#d84f6f";

    public string SecondaryColor { get; set; } = "#7bb8a5";

    public int TokenBalance { get; set; }

    public bool IsPlayerSignedIn { get; set; }

    public string? CompanionName { get; set; }

    public string? CompanionImagePath { get; set; }

    public bool CompanionIsLegendary { get; set; }

    public List<MenuGameViewModel> Games { get; set; } = [];
}
