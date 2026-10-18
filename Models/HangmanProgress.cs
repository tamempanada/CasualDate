using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class HangmanProgress
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    public int AdventureUnlockedLevel { get; set; } = 1;

    [MaxLength(160)]
    public string AdventureCompletedLevels { get; set; } = "";

    public int EndlessTotalWins { get; set; }

    public int EndlessBestStreak { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
