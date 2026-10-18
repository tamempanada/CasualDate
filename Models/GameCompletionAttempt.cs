using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class GameCompletionAttempt
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    [MaxLength(40)]
    public string GameSlug { get; set; } = "";

    [MaxLength(80)]
    public string AttemptToken { get; set; } = "";

    [MaxLength(40)]
    public string Status { get; set; } = GameCompletionAttemptStatus.Started;

    public int? Score { get; set; }

    public int? ResultRangeId { get; set; }

    public int CaptureCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}

public static class GameCompletionAttemptStatus
{
    public const string Started = "Started";
    public const string Completed = "Completed";
}
