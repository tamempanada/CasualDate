namespace CasualDate.ViewModels.Player;

public class PerfectDateGameViewModel
{
    public List<DateCategoryChoiceViewModel> Categories { get; set; } = new();

    public List<DateResultRangeViewModel> ResultRanges { get; set; } = new();
}
