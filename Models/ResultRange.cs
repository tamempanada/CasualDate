using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class ResultRange
{
    public int Id { get; set; }

    public int MiniGameId { get; set; }

    public int MinScore { get; set; }

    public int MaxScore { get; set; }

    [MaxLength(160)]
    public string Title { get; set; } = "";

    public string Message { get; set; } = "";

    public int? MediaId { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
