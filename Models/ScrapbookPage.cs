using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class ScrapbookPage
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    public int PageNumber { get; set; }

    [MaxLength(120)]
    public string Title { get; set; } = "";

    [MaxLength(80)]
    public string DateLabel { get; set; } = "";

    public string Body { get; set; } = "";

    [MaxLength(40)]
    public string PageDesign { get; set; } = "lined";

    [MaxLength(20)]
    public string TextColor { get; set; } = "#3a2724";

    [MaxLength(20)]
    public string TextAlign { get; set; } = "left";

    public int FontSize { get; set; } = 18;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
