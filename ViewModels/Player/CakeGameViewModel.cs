namespace CasualDate.ViewModels.Player;

public class CakeGameViewModel
{
    public List<CakeItemChoiceViewModel> Bases { get; set; } = [];

    public List<CakeItemChoiceViewModel> Frostings { get; set; } = [];

    public List<CakeItemChoiceViewModel> Toppings { get; set; } = [];

    public List<CakeItemChoiceViewModel> Decorations { get; set; } = [];

    public List<DateResultRangeViewModel> ResultRanges { get; set; } = [];
}
