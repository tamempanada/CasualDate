namespace CasualDate.ViewModels.Player;

public class FinalViewModel
{
    public string GameTitle { get; set; } = "Casual Date";

    public string FinalTitle { get; set; } = "You completed our date";

    public string FinalMessage { get; set; } = "";

    public string PrimaryColor { get; set; } = "#d84f6f";

    public string SecondaryColor { get; set; } = "#7bb8a5";

    public List<string> RequiredGameSlugs { get; set; } = new();
}
