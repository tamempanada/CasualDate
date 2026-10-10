using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class CakeRule
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = "";

    public int? BaseItemId { get; set; }

    public int? FrostingItemId { get; set; }

    public int? ToppingItemId { get; set; }

    public int? DecorationItemId { get; set; }

    public int BonusScore { get; set; }

    [MaxLength(160)]
    public string ResultTitle { get; set; } = "";

    public string ResultMessage { get; set; } = "";

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
