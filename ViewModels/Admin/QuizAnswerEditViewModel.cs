using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class QuizAnswerEditViewModel
{
    public int Id { get; set; }

    [MaxLength(300)]
    public string AnswerText { get; set; } = "";

    public int? MediaId { get; set; }

    public int DisplayOrder { get; set; }
}
