using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class TokenRewardSetting
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string EventKey { get; set; } = "";

    [MaxLength(160)]
    public string DisplayName { get; set; } = "";

    public int TokenAmount { get; set; }

    public int TriggerInterval { get; set; } = 1;

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
