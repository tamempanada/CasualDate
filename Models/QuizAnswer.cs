using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class QuizAnswer
{
    public int Id { get; set; }

    public int QuizQuestionId { get; set; }

    [MaxLength(300)]
    public string AnswerText { get; set; } = "";

    public int? MediaId { get; set; }

    public bool IsCorrect { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
