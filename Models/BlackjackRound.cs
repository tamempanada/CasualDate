using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class BlackjackRound
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    [MaxLength(80)]
    public string RoundToken { get; set; } = "";

    public int BetAmount { get; set; }

    [MaxLength(40)]
    public string Status { get; set; } = BlackjackRoundStatus.Active;

    public int PayoutAmount { get; set; }

    public string DeckJson { get; set; } = "[]";

    public string PlayerHandJson { get; set; } = "[]";

    public string DealerHandJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }
}

public static class BlackjackRoundStatus
{
    public const string Active = "Active";
    public const string PlayerBlackjack = "PlayerBlackjack";
    public const string PlayerWin = "PlayerWin";
    public const string DealerWin = "DealerWin";
    public const string Push = "Push";
    public const string PlayerBust = "PlayerBust";
    public const string DealerBust = "DealerBust";
}
