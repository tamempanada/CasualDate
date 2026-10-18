using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class LoveBlocksRun
{
    public int Id { get; set; }
    public int PlayerProfileId { get; set; }
    [MaxLength(80)] public string RunToken { get; set; } = "";
    [MaxLength(80)] public string LastRequestId { get; set; } = "";
    [MaxLength(64)] public string Board { get; set; } = new('.', 64);
    public string PiecesJson { get; set; } = "[]";
    public string ProcessedRequestIdsJson { get; set; } = "[]";
    public int Score { get; set; }
    public int LinesCleared { get; set; }
    public int MovesMade { get; set; }
    public int TokensEarned { get; set; }
    public int ScoreMilestonesAwarded { get; set; }
    public int LineMilestonesAwarded { get; set; }
    public bool PersonalBestRewarded { get; set; }
    public bool IsOver { get; set; }
    public int ScoreRewardAmount { get; set; }
    public int ScoreRewardInterval { get; set; }
    public bool ScoreRewardEnabled { get; set; }
    public int LineRewardAmount { get; set; }
    public int LineRewardInterval { get; set; }
    public bool LineRewardEnabled { get; set; }
    public int BestRewardAmount { get; set; }
    public bool BestRewardEnabled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LoveBlocksPlayerRecord
{
    public int Id { get; set; }
    public int PlayerProfileId { get; set; }
    public int BestScore { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
