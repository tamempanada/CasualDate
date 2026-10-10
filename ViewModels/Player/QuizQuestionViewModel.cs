namespace CasualDate.ViewModels.Player;

public class QuizQuestionViewModel
{
    public int Id { get; set; }

    public string QuestionText { get; set; } = "";

    public string Difficulty { get; set; } = "Easy";

    public int Points { get; set; } = 1;

    public string? ImagePath { get; set; }

    public List<QuizAnswerChoiceViewModel> Answers { get; set; } = new();
}
