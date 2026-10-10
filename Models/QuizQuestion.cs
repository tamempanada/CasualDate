using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class QuizQuestion
{
    public int Id { get; set; }

    public string QuestionText { get; set; } = "";

    public QuizQuestionType QuestionType { get; set; } = QuizQuestionType.MultipleChoice;

    [MaxLength(40)]
    public string Difficulty { get; set; } = "Easy";

    public int Points { get; set; } = 1;

    public int? MediaId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
