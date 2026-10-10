using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class ResultRangeEditViewModel
{
    public int Id { get; set; }

    [Required]
    public int MiniGameId { get; set; }

    public int MinScore { get; set; }

    public int MaxScore { get; set; }

    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = "";

    public string Message { get; set; } = "";

    public int? MediaId { get; set; }

    public int DisplayOrder { get; set; }
}
