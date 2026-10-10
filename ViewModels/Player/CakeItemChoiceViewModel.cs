namespace CasualDate.ViewModels.Player;

public class CakeItemChoiceViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public int Score { get; set; }

    public string Color { get; set; } = "#f6c95f";

    public string? ImagePath { get; set; }
}
