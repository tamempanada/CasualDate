using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class PlayerGameCompletion
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    [MaxLength(120)]
    public string EventKey { get; set; } = "";

    public int TokenAmountAwarded { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
