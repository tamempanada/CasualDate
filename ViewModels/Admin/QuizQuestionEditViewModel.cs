using System.ComponentModel.DataAnnotations;
using CasualDate.Models;

namespace CasualDate.ViewModels.Admin;

public class QuizQuestionEditViewModel
{
    public int Id { get; set; }

    [Required]
    public string QuestionText { get; set; } = "";

    public QuizQuestionType QuestionType { get; set; } = QuizQuestionType.MultipleChoice;

    [Required]
    [MaxLength(40)]
    public string Difficulty { get; set; } = "Easy";

    [Range(1, 100)]
    public int Points { get; set; } = 10;

    public int? MediaId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int CorrectAnswerIndex { get; set; }

    public List<QuizAnswerEditViewModel> Answers { get; set; } = new();
}
