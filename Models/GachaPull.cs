using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class GachaPull
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    public int? GachaPullBatchId { get; set; }

    public int GachaRewardId { get; set; }

    public GachaPullStatus Status { get; set; } = GachaPullStatus.Pending;

    [MaxLength(80)]
    public string PullToken { get; set; } = "";

    public bool IsPityGuaranteed { get; set; }

    public bool IsTenPullGuaranteed { get; set; }

    public int RefundAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public GachaReward? Reward { get; set; }

    public GachaPullBatch? Batch { get; set; }
}
