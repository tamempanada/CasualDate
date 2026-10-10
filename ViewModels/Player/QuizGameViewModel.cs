namespace CasualDate.ViewModels.Player;

public class QuizGameViewModel
{
    public List<QuizQuestionViewModel> Questions { get; set; } = new();

    public List<DateResultRangeViewModel> ResultRanges { get; set; } = new();

    public int MaxScore => Questions.Sum(question => question.Points);
}
