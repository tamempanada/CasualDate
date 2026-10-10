namespace CasualDate.ViewModels.Player;

public class QuizAnswerChoiceViewModel
{
    public int Id { get; set; }

    public string AnswerText { get; set; } = "";

    public string? ImagePath { get; set; }

    public bool IsCorrect { get; set; }
}
